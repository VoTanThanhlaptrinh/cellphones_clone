namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Thông tin để trang thanh toán tự vẽ QR và hiển thị tài khoản nhận (thay vì nhúng trang của cổng).</summary>
public record OnlinePaymentInfo(string? QrCode, string? BankBin, string? AccountNumber, string? AccountName, string? TransferContent);

/// <summary>
/// Payment online của một đơn, lấy từ payment gateway (CellphoneS không lưu). Trang thanh toán dùng <see cref="OrderId"/> trên URL
/// nên tải lại không mất giao dịch. <see cref="Status"/>: PENDING, PAID, FAILED, EXPIRED, CANCELLED hoặc REVIEW_REQUIRED (cần cửa hàng xử lý).
/// <see cref="CheckoutUrl"/>, <see cref="ExpiresAt"/> và <see cref="PaymentInfo"/> chỉ có khi khách còn có thể thanh toán.
/// </summary>
public record OnlinePaymentView(
    long OrderId,
    long Amount,
    string Status,
    string? Provider,
    string? CheckoutUrl,
    DateTime? ExpiresAt,
    OnlinePaymentInfo? PaymentInfo);

/// <summary>Yêu cầu tạo link thanh toán online cho một đơn.</summary>
public record OnlineCheckoutRequest(long OrderId, string Provider);
