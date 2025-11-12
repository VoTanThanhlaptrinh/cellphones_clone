using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;

namespace cellphones_backend.Services;

public interface AuthService
{
    public ApiResponse<string> Register(RegisterDTO register);

    public ApiResponse<string> StudentRegister(StudentRegisterDTO register);
    public ApiResponse<string> TeacherRegister(TeacherRegisterDTO register);

    public ApiResponse<string> Login(LoginDTO loginDTO, HttpRequest httpRequest);

    public Task<User> ReturnUserIfExistEmail(string email);
    public Task<User> ReturnUserIfExistPhone(string phone);

    public string GenerateJwtToken(User user);
}
