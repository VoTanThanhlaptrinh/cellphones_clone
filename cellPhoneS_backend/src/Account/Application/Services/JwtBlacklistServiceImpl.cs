using cellPhoneS_backend.Services.Interface;
using StackExchange.Redis;

namespace cellPhoneS_backend.Services.Implement;

public class JwtBlacklistServiceImpl : IJwtBlacklistService
{
    private readonly IDatabase _redisDb;
    private const string BlacklistPrefix = "jti_blacklist:";
    private const string UserRevokedPrefix = "user-revoked:";

    public JwtBlacklistServiceImpl(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public async Task BlacklistTokenAsync(string jti, DateTime expirationTime)
    {
        var relativeExpiry = expirationTime - DateTime.UtcNow;

        if (relativeExpiry.TotalSeconds > 0)
        {
            // Lưu JTI vào Redis với giá trị là 1 và thời gian sống (TTL)
            await _redisDb.StringSetAsync(
                $"{BlacklistPrefix}{jti}", 
                "1", 
                relativeExpiry
            );
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        if (string.IsNullOrEmpty(jti)) return false;
        
        return await _redisDb.KeyExistsAsync($"{BlacklistPrefix}{jti}");
    }

    public async Task RevokeUserSessionsAsync(string userId)
    {
        await _redisDb.StringSetAsync(
            $"{UserRevokedPrefix}{userId}",
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            cellphones_backend.Services.Implement.JwtTokenServiceImpl.AccessTokenLifetime);
    }

    public async Task<bool> IsUserSessionRevokedAsync(string userId, long? issuedAtUnixSeconds)
    {
        if (string.IsNullOrEmpty(userId)) return false;

        var value = await _redisDb.StringGetAsync($"{UserRevokedPrefix}{userId}");
        if (value.IsNullOrEmpty || !long.TryParse(value.ToString(), out var revokedAt)) return false;

        // Token không có iat (cấp trước khi có mốc thu hồi) không chứng minh được là cấp sau mốc nên bị từ chối.
        // iat chỉ chính xác tới giây: token cấp cùng giây với mốc được coi là cấp sau (để đăng nhập lại ngay sau khi đổi role không bị từ chối nhầm).
        return issuedAtUnixSeconds is null || issuedAtUnixSeconds.Value < revokedAt;
    }
}
