namespace InternsRegister.Application.DTOs
{
    public record DirectionDto(
        Guid Id,
        string Name,
        int InternsCount,
        DateTime CreatedAt
    );
}
