using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
namespace cellphones_backend.Services.Implement;

public class JwtTokenServiceImpl : JwtTokenService
{
    /// <summary>Thời sống của access token; cũng là TTL của mốc thu hồi phiên <c>user-revoked:{userId}</c>.</summary>
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);

    private readonly IConfiguration _configuration;
    private readonly UserService _userService;
    private readonly IDatabase _redisDb;
    private readonly UserManager<User> _userManager;
    private readonly CartService _cartService;
    public JwtTokenServiceImpl(
        IConfiguration configuration,
        UserService userService,
        IConnectionMultiplexer redis,
        UserManager<User> userManager,
        CartService cartService)
    {
        _cartService = cartService;
        _configuration = configuration;
        _userService = userService;
        _redisDb = redis.GetDatabase();
        _userManager = userManager;
    }

    public async Task<string> GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Thêm JTI (Token ID) để quản lý Blacklist
        var jti = Guid.NewGuid().ToString();
        var issuedAt = DateTimeOffset.UtcNow;
        var amountCart = await _cartService.GetAmountCart(user.Id);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            // iat để OnTokenValidated so với mốc thu hồi phiên của user (khoá tài khoản / đổi role).
            new Claim(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(ClaimTypes.Name, user.Fullname ?? ""),
            new Claim("amountCart", amountCart.ToString())
        };

        var roles = await _userService.GetRole(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: issuedAt.UtcDateTime.Add(AccessTokenLifetime), // Access Token chỉ nên sống ngắn (15-30p)
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
    private async Task<JwtRefreshes> ValidateRefreshTokenAsync(string refreshToken, HttpRequest request)
    {
        var redisKey = $"rt:{refreshToken}";
        var jsonValue = await _redisDb.StringGetAsync(redisKey);

        if (jsonValue.IsNullOrEmpty)
        {
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            throw new BadRequestException("Refresh token expired or invalid.");
        }

        JwtRefreshes? storedToken;
        try
        {
            storedToken = JsonSerializer.Deserialize<JwtRefreshes>(jsonValue.ToString());
        }
        catch
        {
            throw new InternalServerException("Token data is corrupted.");
        }

        if (storedToken == null)
        {
            throw new BadRequestException("Invalid token data.");
        }

        if (storedToken.IsUsed)
        {
            await _redisDb.KeyDeleteAsync(redisKey);
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            throw new ForbiddenException("Security alert: Token reuse detected!");
        }

        var currentUserAgent = request.Headers["User-Agent"].ToString();
        if (storedToken.UserAgent != currentUserAgent)
        {
            await _redisDb.KeyDeleteAsync(redisKey);
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            throw new ForbiddenException("Device mismatch detected.");
        }

        return storedToken;
    }
    private string GetRefreshTokenFromCookie(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            throw new BadRequestException("No refresh token provided in cookie.");
        }
        return refreshToken;
    }
    public async Task<string> RefreshJwtToken(HttpRequest request)
    {
        var refreshToken = GetRefreshTokenFromCookie(request);
        var storedToken = await ValidateRefreshTokenAsync(refreshToken, request);

        // Kiểm tra user trước khi xoay vòng token: tài khoản bị xoá/khoá không được cấp refresh token mới
        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null || user.Status != UserStatuses.Active)
        {
            await _redisDb.KeyDeleteAsync($"rt:{refreshToken}");
            request.HttpContext.Response.Cookies.Delete("refreshToken");
            throw new UnauthorizedException(user == null ? "User no longer exists." : "Tài khoản đã bị khoá");
        }

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

        var newAccessToken = await GenerateJwtToken(user);
        return newAccessToken;
    }
    public string ExtractUserId(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token)
           .Claims.First(c => ClaimTypes.NameIdentifier.Equals(c.Type)).Value;
    }

}