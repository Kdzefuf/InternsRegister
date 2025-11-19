using InternsRegister.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace InternsRegister.Application.DTOs;

public record CreateInternDto
{
    [Required(ErrorMessage = "Имя обязательно")]
    [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна")]
    [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Пол обязателен")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string Email { get; set; }

    [RegularExpression(@"^\+7\d{10}$", ErrorMessage = "Телефон должен быть в формате +7XXXXXXXXXX")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Дата рождения обязательна")]
    [Range(typeof(DateOnly), "1900-01-01", "2100-01-01", ErrorMessage = "Некорректная дата рождения")]
    public DateOnly BirthDate { get; set; }

    public string? DirectionName { get; set; }
    public Guid? DirectionId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? ProjectId { get; set; }
}