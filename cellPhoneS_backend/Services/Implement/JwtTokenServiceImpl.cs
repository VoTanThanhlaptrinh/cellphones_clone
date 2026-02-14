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
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.Name, user.Fullname ?? ""),
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
    private async Task<ServiceResult<JwtRefreshes>> ValidateRefreshTokenAsync(string refreshToken, HttpRequest request)
    {
        var redisKey = $"rt:{refreshToken}";
        var jsonValue = await _redisDb.StringGetAsync(redisKey);

        if (jsonValue.IsNullOrEmpty)
        {
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            return ServiceResult<JwtRefreshes>.Fail("Refresh token expired or invalid.", ServiceErrorType.BadRequest);
        }

        JwtRefreshes? storedToken;
        try
        {
            storedToken = JsonSerializer.Deserialize<JwtRefreshes>(jsonValue.ToString());
        }
        catch
        {
            return ServiceResult<JwtRefreshes>.Fail("Token data is corrupted.", ServiceErrorType.General);
        }

        if (storedToken == null)
        {
            return ServiceResult<JwtRefreshes>.Fail("Invalid token data.", ServiceErrorType.BadRequest);
        }

        if (storedToken.IsUsed)
        {
            await _redisDb.KeyDeleteAsync(redisKey);
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            return ServiceResult<JwtRefreshes>.Fail("Security alert: Token reuse detected!", ServiceErrorType.Forbidden);
        }

        var currentUserAgent = request.Headers["User-Agent"].ToString();
        if (storedToken.UserAgent != currentUserAgent)
        {
            await _redisDb.KeyDeleteAsync(redisKey);
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            return ServiceResult<JwtRefreshes>.Fail("Device mismatch detected.", ServiceErrorType.Forbidden);
        }

        return ServiceResult<JwtRefreshes>.Success(storedToken, null);
    }
    private ServiceResult<string> GetRefreshTokenFromCookie(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return ServiceResult<string>.Fail("No refresh token provided in cookie.", ServiceErrorType.BadRequest);
        }
        return ServiceResult<string>.Success(refreshToken, null);
    }
    public async Task<ServiceResult<string>> RefreshJwtToken(HttpRequest request)
    {
        var cookieResult = GetRefreshTokenFromCookie(request);
        if (!cookieResult.IsSuccess)
        {
            return ServiceResult<string>.Fail(cookieResult.Message!, cookieResult.ErrorType);
        }

        var refreshToken = cookieResult.Data!;
        var validationResult = await ValidateRefreshTokenAsync(refreshToken, request);

        if (!validationResult.IsSuccess)
        {
            return ServiceResult<string>.Fail(validationResult.Message!, validationResult.ErrorType);
        }

        var storedToken = validationResult.Data!;

        storedToken.IsUsed = true;
        await SaveRefreshTokenToRedisAsync(refreshToken, storedToken, TimeSpan.FromMinutes(2));

        var currentUserAgent = request.Headers["User-Agent"].ToString();
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
            Secure = request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        request.HttpContext.Response.Cookies.Append("refreshToken", newRefreshTokenGuid, cookieOptions);

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null)
        {
            await _redisDb.KeyDeleteAsync($"rt:{refreshToken}");
            await _redisDb.KeyDeleteAsync($"rt:{newRefreshTokenGuid}");
            return ServiceResult<string>.Fail("User no longer exists.", ServiceErrorType.Unauthorized);
        }

        var newAccessToken = GenerateJwtToken(user);
        return ServiceResult<string>.Success(newAccessToken, "Token refreshed successfully.");
    }
    public string ExtractUserId(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token)
           .Claims.First(c => ClaimTypes.NameIdentifier.Equals(c.Type)).Value;
    }

}