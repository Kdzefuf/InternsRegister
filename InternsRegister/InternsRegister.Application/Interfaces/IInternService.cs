using InternsRegister.Application.DTOs;

namespace InternsRegister.Application.Interfaces
{
    public interface IInternService
    {
        Task<InternDto> CreateAsync(CreateInternDto dto);
        Task UpdateAsync(Guid id, CreateInternDto dto);
        Task DeleteAsync(Guid id);
        Task<List<InternDto>> GetAllAsync();
        Task<PaginatedList<InternDto>> GetPagedAsync(int page, int pageSize, string? direction = null, string? project = null, string? searchQuery = null);

        Task<List<InternDto>> GetByDirectionIdAsync(Guid directionId);
        Task<List<InternDto>> GetByProjectIdAsync(Guid projectId);

        Task<(PaginatedList<InternDto> Interns, List<DirectionDto> Directions, List<ProjectDto> Projects)>
            GetPagedWithFiltersAsync(int pageIndex, int pageSize, string? direction, string? project, string? search);
    }
}
