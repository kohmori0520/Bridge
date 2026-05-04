using System.ComponentModel.DataAnnotations;

namespace Bridge.Api.Dtos.Users;

public class CreateUserRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Sales|Engineer|Admin)$")]
    public string Role { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Sales のみ
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    // Engineer のみ
    public int? PrimarySalesId { get; set; }
}

public class CreateUserResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? SalesId { get; set; }
    public int? EngineerId { get; set; }
}