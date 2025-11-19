namespace InternsRegister.Domain.Entities;

public class Intern
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly BirthDate { get; set; }

    public Guid? InternshipDirectionId { get; set; }
    public Direction? InternshipDirection { get; set; }

    public Guid? CurrentProjectId { get; set; }
    public Project? CurrentProject { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
