using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
namespace cellphones_backend.Services.Implement;

public class JwtTokenServiceImpl : JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserService _userService;
    private readonly IDatabase _redisDb;
    private readonly UserManager<User> _userManager;
    public JwtTokenServiceImpl(
        IConfiguration configuration,
        UserService userService,
        IConnectionMultiplexer redis,
        UserManager<User> userManager)
    {
        _configuration = configuration;
        _userService = userService;
        _redisDb = redis.GetDatabase();
        _userManager = userManager;
    }

    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Thêm JTI (Token ID) để quản lý Blacklist
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        var rolesResult = _userService.GetRole(user).Result;
        if (rolesResult.IsSuccess && rolesResult.Data != null)
        {
            foreach (var role in rolesResult.Data)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), // Access Token chỉ nên sống ngắn (15-30p)
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    public async Task SaveRefreshTokenToRedisAsync(string refreshTokenGuid, JwtRefreshes tokenModel, TimeSpan expiry)
    {
        await _redisDb.StringSetAsync($"rt:{refreshTokenGuid}", JsonSerializer.Serialize(tokenModel), expiry);
    }

    public async Task<string> RefreshJwtToken(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            throw new SecurityTokenException("No refresh token provided in cookie.");
        }

        var redisKey = $"rt:{refreshToken}";
        var jsonValue = await _redisDb.StringGetAsync(redisKey);

        if (jsonValue.IsNullOrEmpty)
        {
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            throw new SecurityTokenException("Refresh token expired or invalid.");
        }

        var storedToken = JsonSerializer.Deserialize<JwtRefreshes>(jsonValue.ToString());

        if (storedToken!.IsUsed)
        {
            await _redisDb.KeyDeleteAsync(redisKey);
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            throw new SecurityTokenException("Security alert: Token reuse detected!");
        }

        var currentUserAgent = request.Headers["User-Agent"].ToString();
        if (storedToken.UserAgent != currentUserAgent)
        {
            await _redisDb.KeyDeleteAsync(redisKey);
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            throw new SecurityTokenException("Device mismatch detected.");
        }

        storedToken.IsUsed = true;
        await SaveRefreshTokenToRedisAsync(refreshToken, storedToken, TimeSpan.FromMinutes(2));

        var newRefreshTokenGuid = Guid.NewGuid().ToString();
        var newTokenModel = new JwtRefreshes
        {
            UserId = storedToken.UserId,
            SessionId = storedToken.SessionId,
            UserAgent = currentUserAgent,
            IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await SaveRefreshTokenToRedisAsync(newRefreshTokenGuid, newTokenModel, TimeSpan.FromDays(7));

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        request.HttpContext.Response.Cookies.Append("refreshToken", newRefreshTokenGuid, cookieOptions);

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        var newAccessToken = GenerateJwtToken(user!);
        return newAccessToken;
    }
    public string ExtractUserId(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token)
           .Claims.First(c => ClaimTypes.NameIdentifier.Equals(c.Type)).Value;
    }
}