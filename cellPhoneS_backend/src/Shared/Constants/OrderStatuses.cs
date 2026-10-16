namespace cellPhoneS_backend.Constants;

/// <summary>Trạng thái của đơn hàng (<c>Order.Status</c>).</summary>
public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Shipping = "Shipping";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}
