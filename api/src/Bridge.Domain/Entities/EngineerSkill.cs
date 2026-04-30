namespace Bridge.Domain.Entities;

public class EngineerSkill
{
    public int Id { get; set; }
    public int EngineerId { get; set; }
    public int SkillId { get; set; }
    public int Years { get; set; }

    // Navigation
    public Engineer Engineer { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}