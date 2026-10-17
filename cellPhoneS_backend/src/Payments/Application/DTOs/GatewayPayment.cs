namespace cellPhoneS_backend.Services.Interface;

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
