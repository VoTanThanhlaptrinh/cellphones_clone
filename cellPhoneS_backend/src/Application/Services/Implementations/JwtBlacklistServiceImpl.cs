using StackExchange.Redis;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement;

public class JwtBlacklistServiceImpl : IJwtBlacklistService
{
    private readonly IDatabase _redisDb;
    private const string BlacklistPrefix = "jti_blacklist:";

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
}