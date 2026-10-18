using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Account;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    /// <summary>
    /// Tài khoản của chính người đang đăng nhập (userId lấy từ token, không nhận từ URL).
    /// Danh sách / chi tiết / khoá người dùng thuộc `/api/admin/users`.
    /// </summary>
    [Route("api/user")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/user/me
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<UserResponse>("User not authenticated", null!));
            }

            var result = await _userService.GetMe(userId);
            return Ok(new ApiResponse<UserResponse>("success", result));
        }

        // PUT: api/user/me
        // Cập nhật họ tên, ngày sinh, số điện thoại
        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateMe([FromBody] UpdateMeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<UserResponse>("User not authenticated", null!));
            }

            var result = await _userService.UpdateMe(userId, request);
            return Ok(new ApiResponse<UserResponse>("Cập nhật thông tin thành công", result));
        }
    }
}
