namespace Bridge.Api.Dtos.Engineers;

public class EngineerListResponse
{
    public List<EngineerSummary> Items { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
}

public class EngineerSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PrimarySalesDto? PrimarySales { get; set; }
    public bool IsAvailable { get; set; }
    public List<EngineerSkillDto> Skills { get; set; } = new();
}

public class PaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
}