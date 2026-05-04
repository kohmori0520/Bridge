namespace Bridge.Api.Dtos.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserSummary User { get; set; } = null!;
}

public class UserSummary
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? SalesId { get; set; }
    public int? EngineerId { get; set; }
}