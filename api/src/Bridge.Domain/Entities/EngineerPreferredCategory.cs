using Bridge.Domain.Enums;

namespace Bridge.Domain.Entities;

public class EngineerPreferredCategory
{
    public int Id { get; set; }
    public int EngineerId { get; set; }
    public SkillCategory Category { get; set; }

    // Navigation
    public Engineer Engineer { get; set; } = null!;
}