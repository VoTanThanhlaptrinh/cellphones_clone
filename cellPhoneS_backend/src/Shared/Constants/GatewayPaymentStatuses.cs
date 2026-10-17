namespace cellPhoneS_backend.Constants;

/// <summary>Trạng thái payment do payment gateway báo về (nguồn sự thật; CellphoneS không lưu lại).</summary>
public static class GatewayPaymentStatuses
{
    public const string Pending = "PENDING";
    public const string Paid = "PAID";
    public const string Failed = "FAILED";
    public const string Expired = "EXPIRED";
    public const string Cancelled = "CANCELLED";
    public const string ReviewRequired = "REVIEW_REQUIRED";

    /// <summary>Đã kết thúc không thành công: khách thử lại thì phải dùng Idempotency-Key mới.</summary>
    public static bool IsFailedTerminal(string status) =>
        status is Failed or Expired or Cancelled;
}
