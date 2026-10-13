using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Elastic.Transport;
namespace cellphones_backend.Services;

public interface AuthService
{
    Task Register(RegisterDTO register, HttpContext context);
    Task<string> StudentRegister(StudentRegisterDTO register);
    Task<string> TeacherRegister(TeacherRegisterDTO register);
    Task<string> Login(LoginDTO loginDTO, HttpContext context);
    Task<string> GenerateJwtToken(User user);
    Task<string> GetInfoAfterLoginByGoogle(HttpContext httpContext);
    Task Logout(HttpContext context);
    Task<string> RefreshToken(HttpContext context);
    Task<string> IsLoggedIn(string userId, HttpContext context);
    Task<string> GetUrlCallbackGoogle();
}
