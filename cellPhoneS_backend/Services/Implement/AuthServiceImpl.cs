using System;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Repository.Interfaces;
using cellPhoneS_backend.Services;
using Elastic.Transport;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Services.Implement;

public class AuthServiceImpl : AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IJwtRotationRepository _jwtRotationRepository;

    public AuthServiceImpl(UserManager<User> userManager, JwtTokenService jwtTokenService, IJwtRotationRepository jwtRotationRepository)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _jwtRotationRepository = jwtRotationRepository;
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
        return _jwtTokenService.GenerateJwtToken(user);
    }

    public async Task<ServiceResult<VoidResponse>> Login(LoginDTO loginDTO, HttpRequest httpRequest)
    {
        var user = await this._userManager.FindByLoginAsync(loginDTO.Phone, loginDTO.Password);
        if (user == null)
        {
            return ServiceResult<VoidResponse>.Fail("Invalid phone or password", ServiceErrorType.Unauthorized);
        }
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript can't read inside
            Secure = true, // Just pass through HTTPS
            SameSite = SameSiteMode.Strict, // Against CSRF
            Expires = DateTime.UtcNow.AddDays(10) // Time life
        };
        var jwtToken = await GenerateJwtToken(user);
        httpRequest.HttpContext.Response.Cookies.Append("jwtToken", jwtToken, cookieOptions);
        return ServiceResult<VoidResponse>.Success(new VoidResponse(), "Login successful");
    }
    public async Task<ServiceResult<VoidResponse>> Register(RegisterDTO register, HttpRequest httpRequest)
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
        httpRequest.HttpContext.Response.Cookies.Append("jwtToken", await GenerateJwtToken(newUser), new CookieOptions
        {
            HttpOnly = true, // JavaScript can't read inside
            Secure = true, // Just pass through HTTPS
            SameSite = SameSiteMode.Strict, // Against CSRF
            Expires = DateTime.UtcNow.AddDays(10) // Time life
        });
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
        var email = user.FindFirst(ClaimTypes.Email)?.Value; // get email of user
        var fullName = user.FindFirst(ClaimTypes.Name)?.Value; // get fullname of user
        var userLogin = await ReturnUserIfExistEmail(email!);
        if (userLogin != null) // Check If user exist then login without register
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // JavaScript can't read inside
                Secure = true, // Just pass through HTTPS
                SameSite = SameSiteMode.Strict, // Against CSRF
                Expires = DateTime.UtcNow.AddDays(10) // Time life
            };
            context.Response.Cookies.Append("jwtToken", (await GenerateJwtToken(userLogin)), cookieOptions);
            // set IsLoginSuccessful = true for frontend pypass register -> login
            return ServiceResult<Oauth2GoogleCallBackResponse>.Success(new Oauth2GoogleCallBackResponse(null, null, true), "Login successful");
        }
        // not exist -> go to Register
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
        context.Response.Cookies.Append("jwtToken", "", cookieOptions);
        return Task.FromResult(ServiceResult<VoidResponse>.Success(new VoidResponse(), "Logout successful"));
    }

    public Task<ServiceResult<VoidResponse>> RefreshToken(HttpContext context)
    {
        context.Request.Cookies.TryGetValue("refresh", out var refresh);
        return Task.FromResult(ServiceResult<VoidResponse>.Success(new VoidResponse(),""));
    }
}
