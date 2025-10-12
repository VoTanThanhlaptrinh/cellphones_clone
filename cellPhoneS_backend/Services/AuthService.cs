using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.DTOs.Responses;

namespace cellphones_backend.Services;

public interface AuthService
{
    public ApiResponse Register(RegisterDTO register);

    public ApiResponse StudentRegister(StudentRegisterDTO register);
    public ApiResponse TeacherRegister(TeacherRegisterDTO register);

    public ApiResponse Login(LoginDTO loginDTO, HttpRequest httpRequest);
}
