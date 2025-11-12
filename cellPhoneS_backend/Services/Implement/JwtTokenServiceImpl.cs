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
        return _jwtSecurityTokenHandler.ReadJwtToken(token) // Đọc ra JwtServiceToken
        .Claims.Where(c => ClaimTypes.NameIdentifier.Equals(c.Type)).ToList()[0].Value; // Lọc ra trường user để lấy Id
    }

    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!)); // lấy secret key tạo ra khóa
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // tạo ra cách ký
        var claims = new List<Claim>{
            new Claim(ClaimTypes.NameIdentifier, user.Id) // thêm Id user vào claims
        };
        foreach (var item in _userService.GetRole(user))
        {
            claims.Add(new Claim(ClaimTypes.Role, item)); // thêm Role user vào claims
        }
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        ); // cấu hình jwt token
        return _jwtSecurityTokenHandler.WriteToken(token);
    }
}
