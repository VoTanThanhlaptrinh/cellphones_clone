using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Quản lý người dùng cho trang quản trị (module Account): danh sách, chi tiết, khoá/mở khoá, gán role.</summary>
public interface IAdminUserService
{
    Task<PagedResult<AdminUserView>> GetUsers(AdminUserFilter filter);
    Task<AdminUserDetail> GetUser(string userId);
    /// <summary>Khoá thì thu hồi mọi access token đã cấp (Redis <c>user-revoked:{id}</c>); login/refresh chặn user không active.</summary>
    Task UpdateStatus(string userId, UpdateUserStatusRequest request, string actorId);
    /// <summary>Thay toàn bộ role của user; trả danh sách role mới (sắp xếp theo tên).</summary>
    Task<List<string>> UpdateRoles(string userId, UpdateUserRolesRequest request, string actorId);
}
