namespace Bridge.Domain.Entities;

public class Sales
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Engineer> Engineers { get; set; } = new List<Engineer>();
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
}