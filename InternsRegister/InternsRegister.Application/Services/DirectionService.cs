using InternsRegister.Application.DTOs;
using InternsRegister.Application.Interfaces;
using InternsRegister.Domain.Entities;
using InternsRegister.Infrastructure.Hubs;
using InternsRegister.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InternsRegister.Application.Services;

public class DirectionService : IDirectionService
{
    private readonly InternsRegisterDbContext _context;
    private readonly IHubContext<InternsHub> _hub;

    public DirectionService(InternsRegisterDbContext context, IHubContext<InternsHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task<DirectionDto> CreateOrGetAsync(string name)
    {
        name = name.Trim();

        var existing = await _context.Directions
            .FirstOrDefaultAsync(d => d.Name == name);

        if (existing != null)
            return MapToDto(existing);

        var direction = new Direction
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Directions.AddAsync(direction);
        await _context.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("InternsUpdated");

        return MapToDto(direction);
    }

    public async Task DeleteAsync(Guid id)
    {
        var direction = await _context.Directions
            .Include(d => d.Interns)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException("Направление не найдено");

        if (direction.Interns.Any())
            throw new InvalidOperationException("Нельзя удалить направление: есть связанные стажёры");

        _context.Directions.Remove(direction);
        await _context.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("InternsUpdated");
    }

    public async Task<List<DirectionDto>> GetAllAsync()
    {
        return await _context.Directions
            .Include(d => d.Interns)
            .OrderBy(d => d.Name)
            .Select(d => MapToDto(d))
            .ToListAsync();
    }

    public async Task<PaginatedList<DirectionDto>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        string? search = null,
        string sortBy = "name",
        bool ascending = true)
    {
        var query = _context.Directions
            .Include(d => d.Interns)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(d => EF.Functions.Like(d.Name, $"%{s}%"));
        }

        query = sortBy.ToLowerInvariant() switch
        {
            "internscount" => ascending
                ? query.OrderBy(d => d.Interns.Count)
                : query.OrderByDescending(d => d.Interns.Count),
            _ => ascending
                ? query.OrderBy(d => d.Name)
                : query.OrderByDescending(d => d.Name)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(d => MapToDto(d))
            .ToListAsync();

        return new PaginatedList<DirectionDto>(items, total, pageIndex, pageSize);
    }

    public async Task UpdateAsync(Guid id, string newName, List<Guid> internIdsToAssign)
    {
        var direction = await _context.Directions
            .Include(d => d.Interns)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException("Направление не найдено");

        newName = newName.Trim();

        if (await _context.Directions.AnyAsync(d => d.Name == newName && d.Id != id))
            throw new InvalidOperationException("Направление с таким именем уже существует");

        foreach (var intern in direction.Interns.ToList())
            intern.InternshipDirectionId = null;

        if (internIdsToAssign.Any())
        {
            var interns = await _context.Interns
                .Where(i => internIdsToAssign.Contains(i.Id))
                .ToListAsync();

            foreach (var intern in interns)
                intern.InternshipDirectionId = direction.Id;
        }

        direction.Name = newName;
        direction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("InternsUpdated");
    }

    private static DirectionDto MapToDto(Direction d) => new(
        d.Id,
        d.Name,
        d.Interns.Count,
        d.CreatedAt
    );
}