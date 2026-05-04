using System.ComponentModel.DataAnnotations;

namespace Bridge.Api.Dtos.EngineerSkills;

public class UpdateEngineerSkillsRequest
{
    [Required]
    public List<EngineerSkillItem> Skills { get; set; } = new();
}

public class EngineerSkillItem
{
    [Required]
    public int SkillId { get; set; }

    [Range(0, 50)]
    public int Years { get; set; }
}