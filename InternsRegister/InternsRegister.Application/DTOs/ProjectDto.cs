namespace InternsRegister.Application.DTOs
{
    public record ProjectDto(
        Guid Id,
        string Name,
        int InternsCount,
        DateTime CreatedAt
    );
}
