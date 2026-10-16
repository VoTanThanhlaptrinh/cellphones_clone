namespace cellPhoneS_backend.Constants;

/// <summary>Khoá cấu hình của payment gateway.</summary>
public static class PaymentConfigKeys
{
    /// <summary>Đặt (user-secrets) để mọi đơn bị tính số tiền này khi thử thanh toán thật; bỏ đi là về tiền thật.</summary>
    public const string TestAmountOverride = "PaymentGateway:TestAmountOverride";
}
