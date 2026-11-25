namespace cellPhoneS_backend.Constants;

public static class RoleNames
{
    public const string Admin = "ADMIN";     // toàn quyền, không sửa/xoá được
    public const string Manager = "MANAGER";
    public const string Staff = "STAFF";
    public const string User = "USER";       // khách hàng, không có permission admin

    /// <summary>Role hệ thống: không xoá, không đổi tên qua API.</summary>
    public static readonly IReadOnlyList<string> System = new[] { Admin, Manager, Staff, User };
}
