using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Role, permission và hồ sơ quản trị của người đang đăng nhập (module Admin).</summary>
public interface IAdminRoleService
{
    Task<AdminMeView> GetMe(string userId);
    List<PermissionView> GetPermissions();
    Task<List<RoleView>> GetRoles();
    Task<RoleView> GetRole(string roleId);
    Task<string> CreateRole(CreateRoleRequest request);
    Task UpdateRole(string roleId, UpdateRoleRequest request);
    Task UpdateRolePermissions(string roleId, UpdateRolePermissionsRequest request);
    Task DeleteRole(string roleId);
    /// <summary>Ném <see cref="ConflictException"/> nếu <paramref name="userId"/> là ADMIN đang hoạt động cuối cùng. Dùng khi khoá user / bỏ role ADMIN.</summary>
    Task EnsureNotLastAdmin(string userId);
}
