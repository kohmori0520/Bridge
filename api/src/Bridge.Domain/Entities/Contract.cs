using Bridge.Domain.Enums;

namespace Bridge.Domain.Entities;

public class Contract : ITimestamped
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public int UnitPrice { get; set; }
    public ContractType ContractType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Assignment Assignment { get; set; } = null!;
}