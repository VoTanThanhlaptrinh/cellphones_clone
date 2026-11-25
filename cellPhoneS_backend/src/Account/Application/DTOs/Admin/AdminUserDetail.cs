namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>
/// Chi tiết người dùng trong trang quản trị. <c>OrderCount</c> tính mọi đơn; <c>TotalSpent</c> chỉ tính đơn đã thu tiền
/// (Paid, Shipping, Completed), gồm cả phí vận chuyển. Hệ thống chưa lưu ngày đăng nhập gần nhất nên không có trường này.
/// </summary>
public class AdminUserDetail
{
    public string Id { get; set; } = "";
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public DateOnly Birth { get; set; }
    public string Status { get; set; } = "";
    public List<string> Roles { get; set; } = new List<string>();
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public int OrderCount { get; set; }
    public double TotalSpent { get; set; }
    /// <summary>Tối đa 5 đơn gần nhất, mới nhất trước.</summary>
    public List<AdminUserOrderView> RecentOrders { get; set; } = new List<AdminUserOrderView>();
}

public record AdminUserOrderView(long Id, DateTime CreateDate, string Status, string Type, double TotalAmount);
