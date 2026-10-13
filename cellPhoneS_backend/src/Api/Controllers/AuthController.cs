using System.Net;
using System.Security.Claims;
using cellphones_backend.DTOs.Account;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.J2O;
using Elastic.Transport;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        [EnableRateLimiting("AuthLimit")]
        public async Task<ActionResult<ApiResponse<VoidResponse>>> Register([FromBody] RegisterDTO register)
        {

            await _authService.Register(register, HttpContext);
            return Ok(new ApiResponse<VoidResponse>("User registered successfully", new VoidResponse()));
        }
        [HttpPost("studentRegister")]
        public async Task<ActionResult<ApiResponse<string>>> StudentRegister([FromBody] StudentRegisterDTO studentRegister)
        {
            var result = await _authService.StudentRegister(studentRegister);
            return Ok(new ApiResponse<string>("Student registered successfully", result));
        }
        [HttpPost("teacherRegister")]
        public async Task<ActionResult<ApiResponse<string>>> TeacherRegister([FromBody] TeacherRegisterDTO teacherRegister)
        {
            var result = await _authService.TeacherRegister(teacherRegister);
            return Ok(new ApiResponse<string>("Teacher registered successfully", result));
        }
        [HttpPost("login")]
        [EnableRateLimiting("AuthLimit")]
        public async Task<ActionResult<ApiResponse<string>>> Login(LoginDTO loginDTO)
        {

            var token = await _authService.Login(loginDTO, HttpContext);
            return Ok(new ApiResponse<string>("Login successful", token));
        }
        [HttpGet("oauth2-google")]
        public async Task Oauth2Google()
        {
            await HttpContext.ChallengeAsync("Google", new AuthenticationProperties
            {
                RedirectUri = await _authService.GetUrlCallbackGoogle(),
            });
        }
        [HttpGet("oauth2-zalo")]
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
        public async Task<ActionResult<ApiResponse<VoidResponse>>> RefreshToken(HttpRequest httpRequest)
        {
            return null!;
        }
        [HttpGet("callbackGoogle")]
        public async Task GetInfoAfterLoginByGoogle()
        {
            string redirectUrl = await _authService.GetInfoAfterLoginByGoogle(HttpContext);
            HttpContext.Response.Redirect(redirectUrl);
        }
        [HttpGet("logout")]
        public async Task<ActionResult<ApiResponse<VoidResponse>>> Logout()
        {

            await _authService.Logout(HttpContext);
            return Ok(new ApiResponse<VoidResponse>("Logout successful", new VoidResponse()));
        }
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<string>>> RefreshToken()
        {

            var token = await _authService.RefreshToken(HttpContext);
            return Ok(new ApiResponse<string>("Token refreshed successfully.", token));
        }
        [HttpPost("isLoggedIn")]
        public async Task<ActionResult<ApiResponse<string>>> IsLoggedIn()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var token = await _authService.IsLoggedIn(userId!, HttpContext);
            return Ok(new ApiResponse<string>("Token refreshed successfully.", token));
        }
    }
}
