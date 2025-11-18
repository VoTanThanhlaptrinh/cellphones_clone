using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface AuthService
{
    Task<ServiceResult<string>> Register(RegisterDTO register);
    Task<ServiceResult<string>> StudentRegister(StudentRegisterDTO register);
    Task<ServiceResult<string>> TeacherRegister(TeacherRegisterDTO register);
    Task<ServiceResult<string>> Login(LoginDTO loginDTO, HttpRequest httpRequest);
    Task<string> GenerateJwtToken(User user);

    Task<ServiceResult<Oauth2GoogleCallBackResponse>> GetInfoAfterLoginByGoogle(HttpContext httpContext);
}
