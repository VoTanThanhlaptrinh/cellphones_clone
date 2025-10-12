using cellphones_backend.DTOs.Account;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
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
        public async Task<ActionResult<ApiResponse>> Register(RegisterDTO register)
        {
            var response = _authService.Register(register);
            await Task.Delay(0);
            return new ActionResult<ApiResponse>(response);
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
        [HttpGet("ok")]
        public IActionResult TestOk()
        {
            return Ok();
        }
    }
}
