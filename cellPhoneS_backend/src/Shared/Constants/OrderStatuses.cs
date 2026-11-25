namespace cellPhoneS_backend.Constants;

/// <summary>Trạng thái của đơn hàng (<c>Order.Status</c>).</summary>
public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Shipping = "Shipping";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    /// <summary>Đơn được tính vào doanh thu (thống kê admin, docs/ADMIN_API_GUIDE.md mục 5.1).</summary>
    public static readonly string[] Revenue = { Paid, Shipping, Completed };
}
