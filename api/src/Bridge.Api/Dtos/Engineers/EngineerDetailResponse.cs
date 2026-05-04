namespace Bridge.Api.Dtos.Engineers;

public class EngineerDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string AvoidedWorkNote { get; set; } = string.Empty;
    public PrimarySalesDto? PrimarySales { get; set; }
    public List<EngineerSkillDto> Skills { get; set; } = new();
    public List<PreferredSkillDto> PreferredSkills { get; set; } = new();
    public List<string> PreferredCategories { get; set; } = new();
    public CurrentContractDto? CurrentContract { get; set; }
}

public class PrimarySalesDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class EngineerSkillDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Years { get; set; }
}

public class PreferredSkillDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class CurrentContractDto
{
    public int AssignmentId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public int UnitPrice { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
    public string RenewalStatus { get; set; } = string.Empty;
}