namespace Bridge.Api.Dtos.ExpiringContracts;

public class ExpiringContractsResponse
{
    public List<ExpiringContractItem> Items { get; set; } = new();
}

public class ExpiringContractItem
{
    public int ContractId { get; set; }
    public int AssignmentId { get; set; }
    public ExpiringEngineerDto Engineer { get; set; } = null!;
    public ExpiringProjectDto Project { get; set; } = null!;
    public DateOnly PeriodTo { get; set; }
    public int DaysRemaining { get; set; }
    public string RenewalStatus { get; set; } = string.Empty;
}

public class ExpiringEngineerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ExpiringProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
}