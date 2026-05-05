using System.ComponentModel.DataAnnotations;

namespace Bridge.Api.Dtos.Projects;

public class ProjectListResponse
{
    public List<ProjectSummary> Items { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class ProjectSummary
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int UnitPriceMin { get; set; }
    public int UnitPriceMax { get; set; }
    public string Status { get; set; } = string.Empty;
    public OwnerSalesDto OwnerSales { get; set; } = null!;
    public int AssignedCount { get; set; }  // 現在アサインされている人数
}

public class OwnerSalesDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProjectDetailResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int UnitPriceMin { get; set; }
    public int UnitPriceMax { get; set; }
    public string Status { get; set; } = string.Empty;
    public OwnerSalesDto OwnerSales { get; set; } = null!;
    public List<RequiredSkillItem> RequiredSkills { get; set; } = new();
}

public class RequiredSkillItem
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;  // "required" or "preferred"
    public int RequiredYears { get; set; }
}

public class CreateProjectRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Range(0, int.MaxValue)]
    public int UnitPriceMin { get; set; }

    [Range(0, int.MaxValue)]
    public int UnitPriceMax { get; set; }

    [RegularExpression("^(draft|open)$")]
    public string Status { get; set; } = "draft";  // "draft" | "open"

    [Required]
    public List<ProjectRequiredSkillItem> RequiredSkills { get; set; } = new();
}

public class UpdateProjectRequest : CreateProjectRequest
{
    [Required]
    [RegularExpression("^(draft|open|closed|cancelled)$")]
    public new string Status { get; set; } = string.Empty;  // "draft" | "open" | "closed" | "cancelled"
}

public class ProjectRequiredSkillItem
{
    public int SkillId { get; set; }
    public string Requirement { get; set; } = "required";  // "required" or "preferred"
    public int RequiredYears { get; set; }
}

public class PaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
}