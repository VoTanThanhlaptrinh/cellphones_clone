using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>Thông tin và permission của tài khoản quản trị đang đăng nhập, để UI ẩn/hiện menu và nút.</summary>
    [Route("api/admin/me")]
    [ApiController]
    [RequirePermission(Permissions.AdminAccess)]
    public class AdminMeController : BaseController
    {
        private readonly IAdminRoleService _adminRoleService;

        public AdminMeController(IAdminRoleService adminRoleService)
        {
            _adminRoleService = adminRoleService;
        }

        // GET: api/admin/me
        [HttpGet]
        public async Task<ActionResult<ApiResponse<AdminMeView>>> GetMe()
        {
            var result = await _adminRoleService.GetMe(CurrentUserId);
            return Ok(new ApiResponse<AdminMeView>("Success", result));
        }
    }
}
