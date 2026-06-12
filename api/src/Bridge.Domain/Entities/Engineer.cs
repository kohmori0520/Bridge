namespace Bridge.Domain.Entities;

public class Engineer : ITimestamped
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? PrimarySalesId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvoidedWorkNote { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Sales? PrimarySales { get; set; }
    public ICollection<EngineerSkill> Skills { get; set; } = new List<EngineerSkill>();
    public ICollection<EngineerPreferredSkill> PreferredSkills { get; set; } = new List<EngineerPreferredSkill>();
    public ICollection<EngineerPreferredCategory> PreferredCategories { get; set; } = new List<EngineerPreferredCategory>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}