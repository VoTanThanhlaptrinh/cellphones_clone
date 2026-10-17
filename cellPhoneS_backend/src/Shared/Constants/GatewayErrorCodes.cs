namespace cellPhoneS_backend.Constants;

/// <summary>Mã lỗi payment gateway trả về (<c>PaymentGatewayException.Code</c>).</summary>
public static class GatewayErrorCodes
{
    public const string PaymentAlreadyPending = "PAYMENT_ALREADY_PENDING";
    public const string PaymentProviderNotEnabled = "PAYMENT_PROVIDER_NOT_ENABLED";
    public const string PaymentProviderNotAllowed = "PAYMENT_PROVIDER_NOT_ALLOWED";
    public const string InvalidAmount = "INVALID_AMOUNT";
    public const string InvalidPaymentRequest = "INVALID_PAYMENT_REQUEST";
    public const string IdempotencyInProgress = "IDEMPOTENCY_IN_PROGRESS";
    public const string PaymentNotFound = "PAYMENT_NOT_FOUND";
    public const string PaymentNotCancellable = "PAYMENT_NOT_CANCELLABLE";
}
