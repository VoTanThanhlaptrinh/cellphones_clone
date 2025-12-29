using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Elastic.Transport;

namespace cellphones_backend.Services;

public interface AuthService
{
    Task<ServiceResult<VoidResponse>> Register(RegisterDTO register, HttpRequest httpRequest);
    Task<ServiceResult<string>> StudentRegister(StudentRegisterDTO register);
    Task<ServiceResult<string>> TeacherRegister(TeacherRegisterDTO register);
    Task<ServiceResult<VoidResponse>> Login(LoginDTO loginDTO, HttpRequest httpRequest);
    Task<string> GenerateJwtToken(User user);
    Task<ServiceResult<Oauth2GoogleCallBackResponse>> GetInfoAfterLoginByGoogle(HttpContext httpContext);
    Task<ServiceResult<VoidResponse>> Logout(HttpContext context);
    Task<ServiceResult<VoidResponse>> RefreshToken(HttpContext context);
}
