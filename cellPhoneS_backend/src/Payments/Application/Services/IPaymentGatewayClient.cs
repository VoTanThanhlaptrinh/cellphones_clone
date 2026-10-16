namespace cellPhoneS_backend.Services.Interface;

/// <summary>Payment trả về từ gateway (xem payment-api.yaml, schema PaymentResponse).</summary>
public record GatewayPayment(
    long PaymentId,
    long OrderId,
    decimal TotalAmount,
    string Provider,
    string Status,
    string? TransactionId,
    string? TransactionStatus,
    string? CheckoutUrl,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? PaidAt,
    GatewayPaymentInfo? PaymentInfo = null);

/// <summary>Thông tin để tự hiển thị thanh toán (payOS): chuỗi VietQR, tài khoản nhận, nội dung chuyển khoản.</summary>
public record GatewayPaymentInfo(string? QrCode, string? BankBin, string? AccountNumber, string? AccountName, string? TransferContent);

/// <summary>Lỗi gateway trả về theo envelope {code, message, traceId}, hoặc lỗi mạng (Status = 0).</summary>
public class PaymentGatewayException : Exception
{
    public int Status { get; }
    public string Code { get; }
    public string? TraceId { get; }
    public double? RetryAfterSeconds { get; init; }

    public PaymentGatewayException(int status, string code, string message, string? traceId = null) : base(message)
    {
        Status = status;
        Code = code;
        TraceId = traceId;
    }
}

/// <summary>Gọi payment gateway (payment-module). Ném <see cref="PaymentGatewayException"/> khi gateway từ chối hoặc không liên lạc được.</summary>
public interface IPaymentGatewayClient
{
    /// <summary>Tạo link thanh toán. Thử lại cùng <paramref name="idempotencyKey"/> và cùng nội dung thì gateway trả lại payment cũ.</summary>
    Task<GatewayPayment> Checkout(long orderId, long amount, string provider, string idempotencyKey);

    /// <summary>Payment mới nhất của đơn; null nếu gateway chưa có payment nào cho đơn (404 PAYMENT_NOT_FOUND).</summary>
    Task<GatewayPayment?> GetStatus(long orderId);

    Task<GatewayPayment> Cancel(long orderId, string? reason);
}
