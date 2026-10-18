namespace cellPhoneS_backend.Services.Interface;

public interface IJwtBlacklistService
{
    Task BlacklistTokenAsync(string jti, DateTime expirationTime);
    Task<bool> IsTokenBlacklistedAsync(string jti);
    /// <summary>Thu hồi mọi access token đã cấp cho user tới thời điểm này (Redis <c>user-revoked:{userId}</c>, TTL = đời access token).</summary>
    Task RevokeUserSessionsAsync(string userId);
    /// <summary>Token cấp lúc <paramref name="issuedAtUnixSeconds"/> (null = token không có iat) có nằm trước mốc thu hồi của user không.</summary>
    Task<bool> IsUserSessionRevokedAsync(string userId, long? issuedAtUnixSeconds);
}
