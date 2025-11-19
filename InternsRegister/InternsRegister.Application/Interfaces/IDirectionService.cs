using InternsRegister.Application.DTOs;

namespace InternsRegister.Application.Interfaces
{
    public interface IDirectionService
    {
        Task<DirectionDto> CreateOrGetAsync(string name);
        Task<List<DirectionDto>> GetAllAsync();
        Task<PaginatedList<DirectionDto>> GetPagedAsync(int page, int pageSize, string? search = null, string sort = "name", bool asc = true);
        Task UpdateAsync(Guid id, string newName, List<Guid> internIds);
        Task DeleteAsync(Guid id);
    }
}
