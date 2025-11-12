using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Services;
using cellPhoneS_backend.DTOs.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet("list/{page}/{pageSize}")]
        public Task<ApiResponse<List<UserResponse>>> GetUsers(int page, int pageSize)
        {
            return _userService.GetUsers(page, pageSize);
        }
        [HttpGet()]
        public IActionResult GetDetails()
        {
            return Ok("Users endpoint is working.");
        }
        [HttpPut()]
        public IActionResult UpdateUser()
        {
            return Ok("Users endpoint is working.");
        }
        [HttpDelete()]
        public IActionResult DeleteUser()
        {
            return Ok("Users endpoint is working.");
        }
    }
}
