using InternsRegister.Application.DTOs;
using InternsRegister.Application.Interfaces;
using InternsRegister.Domain.Entities;
using InternsRegister.Infrastructure.Hubs;
using InternsRegister.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InternsRegister.Application.Services;

public class ProjectService : IProjectService
{
    private readonly InternsRegisterDbContext _context;
    private readonly IHubContext<InternsHub> _hub;

    public ProjectService(InternsRegisterDbContext context, IHubContext<InternsHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task<ProjectDto> CreateOrGetAsync(string name)
    {
        name = name.Trim();

        var existing = await _context.Projects
            .FirstOrDefaultAsync(p => p.Name == name);

        if (existing != null)
            return MapToDto(existing);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("InternsUpdated");

        return MapToDto(project);
    }

    public async Task UpdateAsync(Guid id, string newName, List<Guid> internIdsToAssign)
    {
        var project = await _context.Projects
            .Include(p => p.Interns)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Проект не найден");

        newName = newName.Trim();

        if (await _context.Projects.AnyAsync(p => p.Name == newName && p.Id != id))
            throw new InvalidOperationException("Проект с таким именем уже существует");

        foreach (var intern in project.Interns.ToList())
            intern.CurrentProjectId = null;

        if (internIdsToAssign.Any())
        {
            var interns = await _context.Interns
                .Where(i => internIdsToAssign.Contains(i.Id))
                .ToListAsync();

            foreach (var intern in interns)
                intern.CurrentProjectId = project.Id;
        }

        project.Name = newName;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("InternsUpdated");
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.Interns)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Проект не найден");

        if (project.Interns.Any())
            throw new InvalidOperationException("Нельзя удалить проект: есть связанные стажёры");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("InternsUpdated");
    }

    public async Task<List<ProjectDto>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Interns)
            .OrderBy(p => p.Name)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<PaginatedList<ProjectDto>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        string? search = null,
        string sortBy = "name",
        bool ascending = true)
    {
        var query = _context.Projects
            .Include(p => p.Interns)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{s}%"));
        }

        var total = await query.CountAsync();

        query = sortBy.ToLowerInvariant() switch
        {
            "internscount" => ascending
                ? query.OrderBy(p => p.Interns.Count)
                : query.OrderByDescending(p => p.Interns.Count),
            _ => ascending
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name)
        };

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PaginatedList<ProjectDto>(items, total, pageIndex, pageSize);
    }

    private static ProjectDto MapToDto(Project p) => new(
        p.Id,
        p.Name,
        p.Interns.Count,
        p.CreatedAt
    );
}