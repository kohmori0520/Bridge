namespace Bridge.Api.Dtos.Sales;

public class SalesResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateSalesRequest
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}