namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>Nội dung callback payment gateway POST về website khi một payment đạt trạng thái cuối (payment-module, mục 7 của API_INTEGRATION_GUIDE).</summary>
public record PaymentCallbackPayload(
    string EventId,
    string EventType,
    DateTimeOffset? OccurredAt,
    PaymentCallbackPayment? Payment);

public record PaymentCallbackPayment(
    long PaymentId,
    long OrderId,
    string Status,
    string? Provider,
    decimal Amount,
    string? Currency,
    string? TransactionId,
    string? TransactionStatus,
    DateTimeOffset? PaidAt);
