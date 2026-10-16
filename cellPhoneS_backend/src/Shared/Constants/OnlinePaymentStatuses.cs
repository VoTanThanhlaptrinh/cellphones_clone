namespace cellPhoneS_backend.Constants;

/// <summary>Trạng thái của một lần thanh toán online (<c>OnlinePayment.Status</c>): "Creating" rồi trạng thái do gateway trả về.</summary>
public static class OnlinePaymentStatuses
{
    public const string Creating = "Creating";
    public const string Pending = "PENDING";
    public const string Paid = "PAID";
    public const string ReviewRequired = "REVIEW_REQUIRED";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";

    /// <summary>Trạng thái chưa kết thúc: còn phải hỏi gateway.</summary>
    public static readonly string[] Open = { Creating, Pending, ReviewRequired };
}
