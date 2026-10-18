using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>Người dùng cho trang quản trị: xem (user.read), khoá/mở khoá (user.lock), gán role (user.assign-role).</summary>
    [Route("api/admin/users")]
    [ApiController]
    [RequirePermission(Permissions.UserRead)]
    public class AdminUserController : BaseController
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        // GET: api/admin/users?status=locked&role=STAFF&search=abc&sortBy=email&sortDir=asc
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminUserView>>>> GetUsers([FromQuery] AdminUserFilter filter)
        {
            var result = await _adminUserService.GetUsers(filter);
            return Ok(new ApiResponse<PagedResult<AdminUserView>>("Success", result));
        }

        // GET: api/admin/users/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<AdminUserDetail>>> GetUser(string userId)
        {
            var result = await _adminUserService.GetUser(userId);
            return Ok(new ApiResponse<AdminUserDetail>("Success", result));
        }

        // PATCH: api/admin/users/{userId}/status
        [HttpPatch("{userId}/status")]
        [RequirePermission(Permissions.UserLock)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(string userId, [FromBody] UpdateUserStatusRequest request)
        {
            await _adminUserService.UpdateStatus(userId, request, CurrentUserId);
            return Ok(new ApiResponse<string>("Cập nhật trạng thái tài khoản thành công", ""));
        }

        // PUT: api/admin/users/{userId}/roles
        [HttpPut("{userId}/roles")]
        [RequirePermission(Permissions.UserAssignRole)]
        public async Task<ActionResult<ApiResponse<List<string>>>> UpdateRoles(string userId, [FromBody] UpdateUserRolesRequest request)
        {
            var roles = await _adminUserService.UpdateRoles(userId, request, CurrentUserId);
            return Ok(new ApiResponse<List<string>>("Cập nhật role thành công", roles));
        }
    }
}
