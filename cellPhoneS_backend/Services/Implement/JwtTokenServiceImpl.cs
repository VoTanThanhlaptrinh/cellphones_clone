using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using cellphones_backend.Models;
using cellphones_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace cellPhoneS_backend.Services.Implement;

public class JwtTokenServiceImpl : JwtTokenService
{
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly IConfiguration _configuration;
    private readonly UserService _userService;

    public JwtTokenServiceImpl(IConfiguration configuration, UserService userService)
    {
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        _configuration = configuration;
        _userService = userService;
    }

    public string ExtractUserId(string token)
    {
        return _jwtSecurityTokenHandler.ReadJwtToken(token)
            .Claims.First(c => ClaimTypes.NameIdentifier.Equals(c.Type)).Value;
    }

    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>{ new Claim(ClaimTypes.NameIdentifier, user.Id) };
        // UserService.GetRole now returns ServiceResult<IList<string>>; adapt accordingly
        var rolesResult = _userService.GetRole(user).Result; // synchronous bridging
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
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );
        return _jwtSecurityTokenHandler.WriteToken(token);
    }

    public string HashToken(string token)
    {
        throw new NotImplementedException();
    }
}
