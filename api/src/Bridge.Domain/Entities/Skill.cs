using Bridge.Domain.Enums;

namespace Bridge.Domain.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; }

    // Navigation
    public ICollection<EngineerSkill> EngineerSkills { get; set; } = new List<EngineerSkill>();
    public ICollection<EngineerPreferredSkill> EngineerPreferredSkills { get; set; } = new List<EngineerPreferredSkill>();
    public ICollection<ProjectRequiredSkill> ProjectRequiredSkills { get; set; } = new List<ProjectRequiredSkill>();
}