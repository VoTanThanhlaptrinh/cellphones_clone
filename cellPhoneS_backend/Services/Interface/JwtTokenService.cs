using System;
using cellphones_backend.Models;
using Microsoft.IdentityModel.JsonWebTokens;

namespace cellPhoneS_backend.Services;

public interface JwtTokenService
{
    string GenerateJwtToken(User user);
    string ExtractUserId(string token);
}
