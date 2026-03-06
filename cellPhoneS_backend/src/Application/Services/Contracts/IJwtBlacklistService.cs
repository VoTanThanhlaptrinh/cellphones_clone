using System;

namespace cellPhoneS_backend.Services.Interface;

public interface IJwtBlacklistService
{
    Task BlacklistTokenAsync(string jti, DateTime expirationTime);
    Task<bool> IsTokenBlacklistedAsync(string jti);
}
