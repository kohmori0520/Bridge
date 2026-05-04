namespace Bridge.Api.Dtos.Matching;

// ---- 案件起点(GET /projects/{id}/matches)----

public class ProjectMatchesResponse
{
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public int MaxPossibleScore { get; set; } = 100;
    public List<EngineerMatch> Matches { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class EngineerMatch
{
    public EngineerSummary Engineer { get; set; } = null!;
    public int Score { get; set; }
    public ScoreBreakdown ScoreBreakdown { get; set; } = new();
    public List<SkillEvaluation> SkillEvaluations { get; set; } = new();
    public bool CategoryPreferenceMatched { get; set; }
}

public class EngineerSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PrimarySalesName { get; set; } = string.Empty;
}

// ---- エンジニア起点(GET /engineers/me/matches)----

public class EngineerMatchesResponse
{
    public int EngineerId { get; set; }
    public string EngineerName { get; set; } = string.Empty;
    public List<ProjectMatch> Matches { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class ProjectMatch
{
    public ProjectSummary Project { get; set; } = null!;
    public int Score { get; set; }
    public int MaxPossibleScore { get; set; } = 100;
    public ScoreBreakdown ScoreBreakdown { get; set; } = new();
    public List<SkillEvaluation> SkillEvaluations { get; set; } = new();
    public bool CategoryPreferenceMatched { get; set; }
}

public class ProjectSummary
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public int UnitPriceMin { get; set; }
    public int UnitPriceMax { get; set; }
    public DateOnly StartDate { get; set; }
    public string OwnerSalesName { get; set; } = string.Empty;
}

// ---- 共通 ----

public class ScoreBreakdown
{
    public int SkillMatch { get; set; }
    public int YearsAdequacy { get; set; }
    public int CategoryMatch { get; set; }
}

public class SkillEvaluation
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;  // "required" | "preferred"
    public int RequiredYears { get; set; }
    public int ActualYears { get; set; }
    public string Status { get; set; } = string.Empty;        // "matched" | "insufficient" | "unmet"
}

public class PaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
}