using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Elastic.Transport;
namespace cellphones_backend.Services;

public interface AuthService
{
    Task<ServiceResult<VoidResponse>> Register(RegisterDTO register, HttpContext context);
    Task<ServiceResult<string>> StudentRegister(StudentRegisterDTO register);
    Task<ServiceResult<string>> TeacherRegister(TeacherRegisterDTO register);
    Task<ServiceResult<string>> Login(LoginDTO loginDTO, HttpContext context);
    Task<string> GenerateJwtToken(User user);
    Task<ServiceResult<Oauth2GoogleCallBackResponse>> GetInfoAfterLoginByGoogle(HttpContext httpContext);
    Task<ServiceResult<VoidResponse>> Logout(HttpContext context);
    Task<ServiceResult<string>> RefreshToken(HttpContext context);
}
