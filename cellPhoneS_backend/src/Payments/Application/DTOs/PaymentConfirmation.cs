namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Dữ liệu payment gateway báo về khi một đơn đã được thanh toán (lấy từ GET /status của gateway).</summary>
public record PaymentConfirmation(
    long Amount,
    string Provider,
    string? Reference,
    string? PaymentLinkId,
    string? Description,
    DateTime? TransactionDate);
