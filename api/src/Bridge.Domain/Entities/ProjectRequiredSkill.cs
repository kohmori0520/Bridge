using Bridge.Domain.Enums;

namespace Bridge.Domain.Entities;

public class ProjectRequiredSkill
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int SkillId { get; set; }
    public RequirementType RequirementType { get; set; }
    public int RequiredYears { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}