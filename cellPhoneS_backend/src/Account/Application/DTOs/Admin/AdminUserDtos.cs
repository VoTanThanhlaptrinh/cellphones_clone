using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>
/// Bộ lọc <c>GET /api/admin/users</c>. <c>Status</c> bỏ trống = mọi tài khoản trừ <c>deleted</c>; <c>Role</c> là tên role (vd. STAFF);
/// <c>Search</c> khớp email, họ tên hoặc số điện thoại; <c>SortBy</c>: createDate (mặc định), email, fullName, status.
/// </summary>
public class AdminUserFilter : AdminListQuery
{
    public string? Role { get; set; }
}

/// <summary><c>Status</c>: active hoặc locked. <c>Reason</c> được lưu qua audit log.</summary>
public record UpdateUserStatusRequest(string Status, string? Reason);

/// <summary>Danh sách role mới của user (thay toàn bộ). Role USER luôn được giữ dù không có trong danh sách.</summary>
public record UpdateUserRolesRequest(List<string> Roles);
