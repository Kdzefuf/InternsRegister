using InternsRegister.Domain.Entities;

namespace InternsRegister.Application.DTOs
{
    public record InternDto(
        Guid Id,
        string FirstName,
        string LastName,
        string FullName,
        Gender Gender,
        string Email,
        string? Phone,
        DateOnly BirthDate,
        string? Direction,
        string? Project,
        DateTime CreatedAt
    );
}
