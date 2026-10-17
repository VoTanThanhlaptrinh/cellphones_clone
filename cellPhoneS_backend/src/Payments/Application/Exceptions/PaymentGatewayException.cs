namespace cellPhoneS_backend.Services.Interface;

/// <summary>Status = 0 khi không liên lạc được gateway.</summary>
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
