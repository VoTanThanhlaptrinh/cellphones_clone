namespace cellPhoneS_backend.Models;

public class JwtRefreshes
{
    public string UserId { get; set; } = default!;
    public string SessionId { get; set; } = default!;
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}