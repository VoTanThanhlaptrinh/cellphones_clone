namespace cellPhoneS_backend.Constants;

/// <summary>Khoá cấu hình của payment gateway.</summary>
public static class PaymentConfigKeys
{
    /// <summary>Secret (>= 16 ký tự) khớp <c>callbackSecret</c> của app CELLPHONES ở gateway; dùng kiểm chữ ký callback. Đặt qua user-secrets/biến môi trường.</summary>
    public const string CallbackSecret = "PaymentGateway:CallbackSecret";

    /// <summary>Đặt (user-secrets) để mọi đơn bị tính số tiền này khi thử thanh toán thật; bỏ đi là về tiền thật.</summary>
    public const string TestAmountOverride = "PaymentGateway:TestAmountOverride";
}
