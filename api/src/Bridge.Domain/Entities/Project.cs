using Bridge.Domain.Enums;

namespace Bridge.Domain.Entities;

public class Project : ITimestamped
{
    public int Id { get; set; }
    public int OwnerSalesId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int UnitPriceMin { get; set; }
    public int UnitPriceMax { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Open;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Sales OwnerSales { get; set; } = null!;
    public ICollection<ProjectRequiredSkill> RequiredSkills { get; set; } = new List<ProjectRequiredSkill>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}