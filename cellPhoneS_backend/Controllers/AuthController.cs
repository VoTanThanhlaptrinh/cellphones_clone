using System.Net;
using System.Security.Claims;
using cellphones_backend.DTOs.Account;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Services;
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
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService _authService)
        {
            this._authService = _authService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<string>>> Register(RegisterDTO register)
        {

            return Ok(_authService.Register(register));
        }
        [HttpPost("studentRegister")]
        public IActionResult StudentRegister([FromBody] StudentRegisterDTO studentRegister)
        {
            _authService.StudentRegister(studentRegister);
            return Created();
        }
        [HttpPost("teacherRegister")]
        public IActionResult TeacherRegister([FromBody] TeacherRegisterDTO teacherRegister)
        {
            _authService.TeacherRegister(teacherRegister);
            return Created();
        }
        [HttpPost("login")]
        public IActionResult Login(LoginDTO loginDTO)
        {

            _authService.Login(loginDTO, Request);
            return Ok();
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
        public async Task<ApiResponse<Oauth2GoogleCallBackResponse>> GetInfoAfterLoginByGoogle()
        {
            var context = HttpContext;
            var user = context.User;
            var email = user.FindFirst(ClaimTypes.Email)?.Value; // get email of user
            var fullName = user.FindFirst(ClaimTypes.Name)?.Value; // get fullname of user
            var userLogin = await _authService.ReturnUserIfExistEmail(email!);
            if (userLogin != null) // Check If user exist then login without register
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // JavaScript can't read inside
                    Secure = true, // Just pass through HTTPS
                    SameSite = SameSiteMode.Strict, // Against CSRF
                    Expires = DateTime.UtcNow.AddDays(10) // Time life
                };
                context.Response.Cookies.Append("jwtToken", _authService.GenerateJwtToken(userLogin), cookieOptions);
                // set IsLoginSuccessful = true for frontend pypass register -> login
                return new ApiResponse<Oauth2GoogleCallBackResponse>("Login successful", new Oauth2GoogleCallBackResponse(null, null, true), 200);
            }
            // not exist -> go to Register
            return new ApiResponse<Oauth2GoogleCallBackResponse>("Success", new Oauth2GoogleCallBackResponse(fullName, email, false), 200);
        }
    }
}
