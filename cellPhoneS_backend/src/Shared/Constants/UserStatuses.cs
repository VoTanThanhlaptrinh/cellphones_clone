namespace cellPhoneS_backend.Constants;

/// <summary>Trạng thái tài khoản (<c>User.Status</c>). Chỉ <see cref="Active"/> được đăng nhập / refresh token.</summary>
public static class UserStatuses
{
    public const string Active = "active";
    public const string Locked = "locked";
    public const string Deleted = "deleted";
}
