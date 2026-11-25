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
    /// <summary>Quản lý role và permission. Mọi action cần `role.manage`; permission lưu ở AspNetRoleClaims.</summary>
    [Route("api/admin")]
    [ApiController]
    [RequirePermission(Permissions.RoleManage)]
    public class AdminRoleController : BaseController
    {
        private readonly IAdminRoleService _adminRoleService;

        public AdminRoleController(IAdminRoleService adminRoleService)
        {
            _adminRoleService = adminRoleService;
        }

        // GET: api/admin/permissions
        [HttpGet("permissions")]
        public ActionResult<ApiResponse<List<PermissionView>>> GetPermissions()
        {
            return Ok(new ApiResponse<List<PermissionView>>("Success", _adminRoleService.GetPermissions()));
        }

        // GET: api/admin/roles
        [HttpGet("roles")]
        public async Task<ActionResult<ApiResponse<List<RoleView>>>> GetRoles()
        {
            var result = await _adminRoleService.GetRoles();
            return Ok(new ApiResponse<List<RoleView>>("Success", result));
        }

        // GET: api/admin/roles/{roleId}
        [HttpGet("roles/{roleId}")]
        public async Task<ActionResult<ApiResponse<RoleView>>> GetRole(string roleId)
        {
            var result = await _adminRoleService.GetRole(roleId);
            return Ok(new ApiResponse<RoleView>("Success", result));
        }

        // POST: api/admin/roles
        [HttpPost("roles")]
        public async Task<ActionResult<ApiResponse<string>>> CreateRole([FromBody] CreateRoleRequest request)
        {
            var id = await _adminRoleService.CreateRole(request);
            return Ok(new ApiResponse<string>("Role created successfully", id));
        }

        // PUT: api/admin/roles/{roleId}
        // Renames a custom role (system roles cannot be renamed)
        [HttpPut("roles/{roleId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateRole(string roleId, [FromBody] UpdateRoleRequest request)
        {
            await _adminRoleService.UpdateRole(roleId, request);
            return Ok(new ApiResponse<string>("Role updated successfully", ""));
        }

        // PUT: api/admin/roles/{roleId}/permissions
        // Replaces the whole permission set of the role (ADMIN and USER cannot be edited)
        [HttpPut("roles/{roleId}/permissions")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateRolePermissions(string roleId, [FromBody] UpdateRolePermissionsRequest request)
        {
            await _adminRoleService.UpdateRolePermissions(roleId, request);
            return Ok(new ApiResponse<string>("Role permissions updated successfully", ""));
        }

        // DELETE: api/admin/roles/{roleId}
        [HttpDelete("roles/{roleId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteRole(string roleId)
        {
            await _adminRoleService.DeleteRole(roleId);
            return Ok(new ApiResponse<string>("Role deleted successfully", ""));
        }
    }
}
