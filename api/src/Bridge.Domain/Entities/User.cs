using Bridge.Domain.Enums;

namespace Bridge.Domain.Entities;

public class User : ITimestamped
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Sales? Sales { get; set; }
    public Engineer? Engineer { get; set; }
}