namespace Bridge.Domain.Entities;

public class EngineerPreferredSkill
{
    public int Id { get; set; }
    public int EngineerId { get; set; }
    public int SkillId { get; set; }

    // Navigation
    public Engineer Engineer { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}