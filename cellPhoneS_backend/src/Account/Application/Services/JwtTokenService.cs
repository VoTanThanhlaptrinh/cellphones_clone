using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellPhoneS_backend.Services;

public interface JwtTokenService
{
    Task<string> GenerateJwtToken(User user);
    string ExtractUserId(string token);
    string HashToken(string token);
    Task<string> RefreshJwtToken(HttpRequest request);
    Task SaveRefreshTokenToRedisAsync(string refreshTokenGuid, JwtRefreshes tokenModel, TimeSpan expiry);
}
