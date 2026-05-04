using System.ComponentModel.DataAnnotations;

namespace Bridge.Api.Dtos.EngineerPreferences;

public class EngineerPreferencesResponse
{
    public List<PreferredSkillItem> PreferredSkills { get; set; } = new();
    public List<string> PreferredCategories { get; set; } = new();
    public string AvoidedWorkNote { get; set; } = string.Empty;
}

public class PreferredSkillItem
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class UpdateEngineerPreferencesRequest
{
    [Required]
    public List<int> PreferredSkillIds { get; set; } = new();

    [Required]
    public List<string> PreferredCategories { get; set; } = new();

    [MaxLength(2000)]
    public string AvoidedWorkNote { get; set; } = string.Empty;
}