using InternsRegister.Application.DTOs;

namespace InternsRegister.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDto> CreateOrGetAsync(string name);
        Task<List<ProjectDto>> GetAllAsync();
        Task<PaginatedList<ProjectDto>> GetPagedAsync(int page, int pageSize, string? search = null, string sort = "name", bool asc = true);
        Task UpdateAsync(Guid id, string newName, List<Guid> internIds);
        Task DeleteAsync(Guid id);
    }
}
