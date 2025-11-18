using System.Net;
using System.Security.Claims;
using cellphones_backend.DTOs.Account;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.J2O;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly AuthService _authService;
        public AuthController(AuthService _authService)
        {
            this._authService = _authService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<string>>> Register(RegisterDTO register)
        {

            return HandleResult(await _authService.Register(register));
        }
        [HttpPost("studentRegister")]
        public async Task<ActionResult<ApiResponse<string>>> StudentRegister([FromBody] StudentRegisterDTO studentRegister)
        {
            return HandleResult(await _authService.StudentRegister(studentRegister));
        }
        [HttpPost("teacherRegister")]
        public async Task<ActionResult<ApiResponse<string>>> TeacherRegister([FromBody] TeacherRegisterDTO teacherRegister)
        {
            return HandleResult(await _authService.TeacherRegister(teacherRegister));
        }
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<string>>> Login(LoginDTO loginDTO)
        {

            return HandleResult(await _authService.Login(loginDTO, Request));
        }
        [HttpGet("Oauth2-google")]
        public Task Oauth2Google()
        {
            // After login by Google success -> redirect to URL of frontend
            return HttpContext.ChallengeAsync("Google", new AuthenticationProperties
            {
                // URL of frontend -> Frontend will call API callBack/google for getting data include {email,name}
                RedirectUri = "http://localhost:4434/auth-handler",
            });
        }
        [HttpGet("oauth2Zalo")]
        public IActionResult Oauth2Zalo()
        {
            return Ok();
        }
        [HttpPost("changePass")]
        public async Task<ActionResult<string>> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            return null!;
        }
        [HttpGet("refreshToken")]
        public async Task<ActionResult<ApiResponse<string>>> RefreshToken()
        {
            return null!;
        }
        [HttpGet("callBack/google")]
        [Authorize] // makesure users have to login to get permission access
        public async Task<ActionResult<ApiResponse<Oauth2GoogleCallBackResponse>>> GetInfoAfterLoginByGoogle()
        {
            return HandleResult(await _authService.GetInfoAfterLoginByGoogle(HttpContext));
        }
    }
}
