using System;
using System.Threading.Tasks;
using cellphones_backend.DTOs.Account;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
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

    public async Task<User> ReturnUserIfExistEmail(string email)
    {
        return _userManager.FindByEmailAsync(email).Result!;
    }

    public async Task<User> ReturnUserIfExistPhone(string phone)
    {
        return await _userManager.Users.FirstAsync(x => x.PhoneNumber == phone);
    }

    public string GenerateJwtToken(User user)
    {
        return _jwtTokenService.GenerateJwtToken(user);
    }

    public ApiResponse<string> Login(LoginDTO loginDTO, HttpRequest httpRequest)
    {
        throw new NotImplementedException();
    }

    public ApiResponse<string> Register(RegisterDTO register)
    {
        return null!;
    }

    public ApiResponse<string> StudentRegister(StudentRegisterDTO register)
    {
        throw new NotImplementedException();
    }

    public ApiResponse<string> TeacherRegister(TeacherRegisterDTO register)
    {
        throw new NotImplementedException();
    }
}
