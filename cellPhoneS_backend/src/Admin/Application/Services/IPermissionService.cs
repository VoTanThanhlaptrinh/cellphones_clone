namespace cellPhoneS_backend.Services.Interface;

public interface IPermissionService
{
    /// <summary>Hợp các permission của những role đã cho (tên role lấy từ JWT).</summary>
    Task<IReadOnlySet<string>> GetPermissionsAsync(IEnumerable<string> roleNames);
    /// <summary>Gọi sau khi sửa permission/xoá/đổi tên role để request kế tiếp đọc lại từ DB.</summary>
    void Invalidate(string roleName);
}
