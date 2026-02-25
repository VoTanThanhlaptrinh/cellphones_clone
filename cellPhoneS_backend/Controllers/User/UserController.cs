using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet("list/{page}/{pageSize}")]
        public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetUsers(int page, int pageSize)
        {
            return HandleResult(await _userService.GetUsers(page, pageSize));
        }
        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<cellphones_backend.Models.User>>> GetDetails(string userId)
        {
            return HandleResult(await _userService.GetDetails(userId));
        }
        [HttpPut()]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser()
        {
            return null!;
        }
        [HttpDelete("{userId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteUser(string userId)
        {
            return HandleResult(await _userService.DeleteUser(userId));
        }
    }
}
