namespace Bridge.Api.Dtos.Auth;

public class MeResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? SalesId { get; set; }
    public int? EngineerId { get; set; }
    public string? Name { get; set; }
}