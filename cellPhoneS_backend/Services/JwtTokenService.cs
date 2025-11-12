using System;
using cellphones_backend.Models;
using Microsoft.IdentityModel.JsonWebTokens;

namespace cellPhoneS_backend.Services;

public interface JwtTokenService
{
    public string GenerateJwtToken(User user);

    public string ExtractUserId(string token);

}
