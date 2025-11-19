using InternsRegister.Application.DTOs;
using InternsRegister.Application.Interfaces;
using InternsRegister.Domain.Entities;
using InternsRegister.Infrastructure.Hubs;
using InternsRegister.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InternsRegister.Application.Services;

public class InternService : IInternService
{
    private readonly InternsRegisterDbContext _context;
    private readonly IDirectionService _directionService;
    private readonly IProjectService _projectService;
    private readonly IHubContext<InternsHub> _hub;

    public InternService(
        InternsRegisterDbContext context,
        IDirectionService directionService,
        IProjectService projectService,
        IHubContext<InternsHub> hub)
    {
        _context = context;
        _directionService = directionService;
        _projectService = projectService;
        _hub = hub;
    }

    public async Task<InternDto> CreateAsync(CreateInternDto dto)
    {
        dto.FirstName = dto.FirstName;
        dto.LastName = dto.LastName;
        dto.Email = dto.Email;
        dto.Phone = dto.Phone?.Trim();

        var existingEmail = await _context.Interns
            .AnyAsync(i => i.Email == dto.Email);
        if (existingEmail)
            throw new InvalidOperationException($"Email '{dto.Email}' уже используется");

        if (!string.IsNullOrEmpty(dto.Phone))
        {
            if (!dto.Phone.StartsWith("+7") || dto.Phone.Length != 12 || !long.TryParse(dto.Phone[2..], out _))
                throw new InvalidOperationException("Телефон должен быть в формате +7XXXXXXXXXX");

            var existingPhone = await _context.Interns
                .AnyAsync(i => i.Phone == dto.Phone);
            if (existingPhone)
                throw new InvalidOperationException($"Телефон '{dto.Phone}' уже используется");
        }

        Guid? directionId;
        if (dto.DirectionId.HasValue)
        {
            directionId = dto.DirectionId.Value;
            var directionExists = await _context.Directions
                .AnyAsync(d => d.Id == directionId);
            if (!directionExists)
                throw new InvalidOperationException("Выбранное направление не найдено");
        }
        else if (!string.IsNullOrWhiteSpace(dto.DirectionName))
        {
            var directionDto = await _directionService.CreateOrGetAsync(dto.DirectionName.Trim());
            directionId = directionDto.Id;
        }
        else
        {
            throw new InvalidOperationException("Не указано направление стажировки");
        }

        Guid? projectId;
        if (dto.ProjectId.HasValue)
        {
            projectId = dto.ProjectId.Value;
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId);
            if (!projectExists)
                throw new InvalidOperationException("Выбранный проект не найден");
        }
        else if (!string.IsNullOrWhiteSpace(dto.ProjectName))
        {
            var projectDto = await _projectService.CreateOrGetAsync(dto.ProjectName.Trim());
            projectId = projectDto.Id;
        }
        else
        {
            throw new InvalidOperationException("Не указан проект стажировки");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        var intern = new Intern
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Gender = dto.Gender,
            Email = dto.Email,
            Phone = dto.Phone,
            BirthDate = dto.BirthDate,
            InternshipDirectionId = directionId,
            CurrentProjectId = projectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Interns.AddAsync(intern);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _hub.Clients.All.SendAsync("InternsUpdated");

        var savedIntern = await _context.Interns
            .Include(i => i.InternshipDirection)
            .Include(i => i.CurrentProject)
            .FirstOrDefaultAsync(i => i.Id == intern.Id);

        return MapToDto(savedIntern!);
    }

    public async Task UpdateAsync(Guid id, CreateInternDto dto)
    {
        var intern = await _context.Interns
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException("Стажер не найден");

        dto.FirstName = dto.FirstName.Trim();
        dto.LastName = dto.LastName.Trim();
        dto.Email = dto.Email.Trim().ToLower();
        dto.Phone = dto.Phone?.Trim();

        if (await _context.Interns.AnyAsync(i => i.Email == dto.Email && i.Id != id))
            throw new InvalidOperationException("Email уже используется");

        if (!string.IsNullOrWhiteSpace(dto.Phone) &&
            await _context.Interns.AnyAsync(i => i.Phone == dto.Phone && i.Id != id))
            throw new InvalidOperationException("Телефон уже используется");

        Guid directionId;
        if (dto.DirectionId.HasValue)
        {
            directionId = dto.DirectionId.Value;
            var directionExists = await _context.Directions
                .AnyAsync(d => d.Id == directionId);
            if (!directionExists)
                throw new InvalidOperationException("Выбранное направление не найдено");
        }
        else if (!string.IsNullOrWhiteSpace(dto.DirectionName))
        {
            var directionDto = await _directionService.CreateOrGetAsync(dto.DirectionName.Trim());
            directionId = directionDto.Id;
        }
        else
        {
            throw new InvalidOperationException("Не указано направление стажировки");
        }

        Guid? projectId;
        if (dto.ProjectId.HasValue)
        {
            projectId = dto.ProjectId.Value;
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId);
            if (!projectExists)
                throw new InvalidOperationException("Выбранный проект не найден");
        }
        else if (!string.IsNullOrWhiteSpace(dto.ProjectName))
        {
            var projectDto = await _projectService.CreateOrGetAsync(dto.ProjectName.Trim());
            projectId = projectDto.Id;
        }
        else
        {
            throw new InvalidOperationException("Не указан проект стажировки");
        }

        intern.FirstName = dto.FirstName;
        intern.LastName = dto.LastName;
        intern.Gender = dto.Gender;
        intern.Email = dto.Email;
        intern.Phone = dto.Phone;
        intern.BirthDate = dto.BirthDate;
        intern.InternshipDirectionId = directionId;
        intern.CurrentProjectId = projectId;
        intern.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("InternsUpdated");
    }

    public async Task DeleteAsync(Guid id)
    {
        var intern = await _context.Interns.FindAsync(id);
        if (intern == null) throw new KeyNotFoundException();

        _context.Interns.Remove(intern);
        await _context.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("InternsUpdated");
    }

    public async Task<List<InternDto>> GetAllAsync()
        => await _context.Interns
            .Include(i => i.InternshipDirection)
            .Include(i => i.CurrentProject)
            .OrderBy(i => i.LastName)
            .ThenBy(i => i.FirstName)
            .Select(i => MapToDto(i))
            .ToListAsync();

    public async Task<PaginatedList<InternDto>> GetPagedAsync(
    int pageIndex,
    int pageSize,
    string? directionFilter = null,
    string? projectFilter = null,
    string? searchQuery = null)
    {
        var query = _context.Interns
            .Include(i => i.InternshipDirection)
            .Include(i => i.CurrentProject)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(directionFilter))
            query = query.Where(i => i.InternshipDirection!.Name == directionFilter);

        if (!string.IsNullOrWhiteSpace(projectFilter))
            query = query.Where(i => i.CurrentProject!.Name == projectFilter);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.Trim().ToLower();
            query = query.Where(i =>
                i.FirstName.ToLower().Contains(search) ||
                i.LastName.ToLower().Contains(search) ||
                i.Email.ToLower().Contains(search) ||
                (i.Phone != null && i.Phone.Contains(search)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(i => i.LastName)
            .ThenBy(i => i.FirstName)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(i => MapToDto(i))
            .ToListAsync();

        return new PaginatedList<InternDto>(items, total, pageIndex, pageSize);
    }

    private static InternDto MapToDto(Intern i) => new(
        i.Id,
        i.FirstName,
        i.LastName,
        $"{i.FirstName} {i.LastName}",
        i.Gender,
        i.Email,
        i.Phone,
        i.BirthDate,
        i.InternshipDirection?.Name,
        i.CurrentProject?.Name,
        i.CreatedAt);

    public async Task<List<InternDto>> GetByDirectionIdAsync(Guid directionId)
    {
        return await _context.Interns
            .Where(i => i.InternshipDirectionId == directionId)
            .Include(i => i.InternshipDirection)
            .Include(i => i.CurrentProject)
            .OrderBy(i => i.LastName)
            .ThenBy(i => i.FirstName)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<List<InternDto>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Interns
            .Where(i => i.CurrentProjectId == projectId)
            .Include(i => i.InternshipDirection)
            .Include(i => i.CurrentProject)
            .OrderBy(i => i.LastName)
            .ThenBy(i => i.FirstName)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<(PaginatedList<InternDto> Interns, List<DirectionDto> Directions, List<ProjectDto> Projects)> GetPagedWithFiltersAsync(
        int pageIndex,
        int pageSize,
        string? directionFilter,
        string? projectFilter,
        string? searchQuery)
    {
        var query = _context.Interns
            .Include(i => i.InternshipDirection)
            .Include(i => i.CurrentProject)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(directionFilter))
            query = query.Where(i => i.InternshipDirection!.Name == directionFilter);

        if (!string.IsNullOrWhiteSpace(projectFilter))
            query = query.Where(i => i.CurrentProject!.Name == projectFilter);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.Trim().ToLower();
            query = query.Where(i =>
                i.FirstName.ToLower().Contains(search) ||
                i.LastName.ToLower().Contains(search) ||
                i.Email.ToLower().Contains(search) ||
                (i.Phone != null && i.Phone.Contains(search)));
        }

        var total = await query.CountAsync();

        var interns = await query
            .OrderBy(i => i.LastName)
            .ThenBy(i => i.FirstName)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(i => MapToDto(i))
            .ToListAsync();

        var directions = await _context.Directions
            .Select(d => new DirectionDto(d.Id, d.Name, 0, d.CreatedAt))
            .ToListAsync();

        var projects = await _context.Projects
            .Select(p => new ProjectDto(p.Id, p.Name, 0, p.CreatedAt))
            .ToListAsync();

        var paged = new PaginatedList<InternDto>(interns, total, pageIndex, pageSize);

        return (paged, directions, projects);
    }
}