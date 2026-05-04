using System.ComponentModel.DataAnnotations;

namespace Bridge.Api.Dtos.Assignments;

// ---- リクエスト ----

public class CreateAssignmentRequest
{
    [Required]
    public int EngineerId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public DateOnly AssignedAt { get; set; }

    [Required]
    public InitialContractRequest InitialContract { get; set; } = new();
}

public class InitialContractRequest
{
    [Required]
    public DateOnly PeriodFrom { get; set; }

    [Required]
    public DateOnly PeriodTo { get; set; }

    [Range(0, int.MaxValue)]
    public int UnitPrice { get; set; }
}

public class UpdateAssignmentRequest
{
    [Required]
    [RegularExpression("^(active|ended|cancelled)$")]
    public string Status { get; set; } = string.Empty;
}

public class CreateContractRequest
{
    [Required]
    public DateOnly PeriodFrom { get; set; }

    [Required]
    public DateOnly PeriodTo { get; set; }

    [Range(0, int.MaxValue)]
    public int UnitPrice { get; set; }
}

// ---- レスポンス ----

public class AssignmentDetailResponse
{
    public int Id { get; set; }
    public int EngineerId { get; set; }
    public string EngineerName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly AssignedAt { get; set; }
    public List<ContractItem> Contracts { get; set; } = new();
}

public class ContractItem
{
    public int Id { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public int UnitPrice { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class EngineerAssignmentsResponse
{
    public List<AssignmentWithContracts> Assignments { get; set; } = new();
}

public class AssignmentWithContracts
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly AssignedAt { get; set; }
    public List<ContractItem> Contracts { get; set; } = new();
}

public class CreateContractResult
{
    public bool Success { get; set; }
    public ContractItem? Contract { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CreateAssignmentResult
{
    public bool Success { get; set; }
    public AssignmentDetailResponse? Assignment { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}