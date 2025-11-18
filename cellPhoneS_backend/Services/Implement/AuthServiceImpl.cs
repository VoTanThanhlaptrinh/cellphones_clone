using System;
using System.Security.Claims;
using System.Threading.Tasks;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
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
        return _jwtTokenService.GenerateJwtToken(user);
    }

    public Task<ServiceResult<string>> Login(LoginDTO loginDTO, HttpRequest httpRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> Register(RegisterDTO register)
    {
        throw new NotImplementedException();
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
                return  ServiceResult<Oauth2GoogleCallBackResponse>.Success(new Oauth2GoogleCallBackResponse(null, null, true),"Login successful");
            }
            // not exist -> go to Register
            return ServiceResult<Oauth2GoogleCallBackResponse>.Fail("User not found, need to register", ServiceErrorType.NotFound);
    }
}
