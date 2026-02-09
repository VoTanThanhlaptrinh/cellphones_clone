using System;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services;
using Elastic.Transport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Services.Implement;

public class AuthServiceImpl : AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtTokenService _jwtTokenService;

    public AuthServiceImpl(UserManager<User> userManager, JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    private async Task<User> ReturnUserIfExistEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null!;
        return user;
    }

    private async Task<User> ReturnUserIfExistPhone(string phone)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
        if (user == null) return null!;
        return user;
    }

    public async Task<string> GenerateJwtToken(User user)
    {
        // JwtTokenService reverted to string return
        return  _jwtTokenService.GenerateJwtToken(user);
    }

    public async Task<ServiceResult<string>> Login(LoginDTO loginDTO, HttpContext context)
    {
        var user = await ReturnUserIfExistEmail(loginDTO.Email);
        if (user == null)
        {
            return ServiceResult<string>.Fail("Email not found", ServiceErrorType.Unauthorized);
        }

        if (!await _userManager.CheckPasswordAsync(user, loginDTO.Password))
        {
            return ServiceResult<string>.Fail("Incorrect password", ServiceErrorType.Unauthorized);
        }

        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var refreshToken = Guid.NewGuid().ToString();
        var sessionId = Guid.NewGuid().ToString();

        var tokenModel = new JwtRefreshes
        {
            UserId = user.Id,
            SessionId = sessionId,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
        
        await _jwtTokenService.SaveRefreshTokenToRedisAsync(refreshToken, tokenModel, TimeSpan.FromDays(7));

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        context.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        var jwtToken = await GenerateJwtToken(user);

        return ServiceResult<string>.Success(jwtToken, "Login successful");
    }
    public async Task<ServiceResult<VoidResponse>> Register(RegisterDTO register, HttpContext context)
    {
        var user = await ReturnUserIfExistPhone(register.Phone);
        if (user != null)
        {
            return ServiceResult<VoidResponse>.Fail("Phone already exists", ServiceErrorType.BadRequest);
        }
        user = await ReturnUserIfExistEmail(register.Email);
        if (user != null)
        {
            return ServiceResult<VoidResponse>.Fail("Email already exists", ServiceErrorType.BadRequest);
        }
        var newUser = new User
        {
            UserName = register.Email,
            Email = register.Email,
            PhoneNumber = register.Phone,
            Fullname = register.FullName,
            Birth = register.BirthDay,
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            Status = "active"
        };
        this._userManager.CreateAsync(newUser, register.Password).Wait();
        return ServiceResult<VoidResponse>.Success(new VoidResponse(), "User registered successfully");
    }

    public Task<ServiceResult<string>> StudentRegister(StudentRegisterDTO register)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> TeacherRegister(TeacherRegisterDTO register)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<Oauth2GoogleCallBackResponse>> GetInfoAfterLoginByGoogle(HttpContext context)
    {
        var user = context.User;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        var fullName = user.FindFirst(ClaimTypes.Name)?.Value;
        var userLogin = await ReturnUserIfExistEmail(email!);
        if (userLogin != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // JavaScript can't read inside
                Secure = true, // Just pass through HTTPS
                SameSite = SameSiteMode.Strict, // Against CSRF
                Expires = DateTime.UtcNow.AddDays(10) // Time life
            };
            var jwtToken = await GenerateJwtToken(userLogin);
            string refreshToken = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            return ServiceResult<Oauth2GoogleCallBackResponse>.Success(new Oauth2GoogleCallBackResponse(null, null, true), "Login successful");
        }
        return ServiceResult<Oauth2GoogleCallBackResponse>.Fail("User not found, need to register", ServiceErrorType.NotFound);
    }

    public Task<ServiceResult<VoidResponse>> Logout(HttpContext context)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript can't read inside
            Secure = true, // Just pass through HTTPS
            SameSite = SameSiteMode.Strict, // Against CSRF
            Expires = DateTime.UtcNow.AddDays(-1) // Expire the cookie
        };
        context.Response.Cookies.Append("refreshToken", "", cookieOptions);
        return Task.FromResult(ServiceResult<VoidResponse>.Success(new VoidResponse(), "Logout successful"));
    }

    public async Task<ServiceResult<string>> RefreshToken(HttpContext context)
    {
        var newJwtToken = await _jwtTokenService.RefreshJwtToken(context.Request);
        return ServiceResult<string>.Success(newJwtToken, "Token refreshed successfully");
    }
}
