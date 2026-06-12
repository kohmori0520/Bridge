using Bridge.Domain.Enums;

namespace Bridge.Domain.Entities;

public class Assignment : ITimestamped
{
    public int Id { get; set; }
    public int EngineerId { get; set; }
    public int ProjectId { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Active;
    public DateOnly AssignedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Engineer Engineer { get; set; } = null!;
    public Project Project { get; set; } = null!;
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}