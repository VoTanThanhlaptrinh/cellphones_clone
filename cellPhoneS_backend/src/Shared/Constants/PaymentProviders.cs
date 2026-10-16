namespace cellPhoneS_backend.Constants;

/// <summary>Cổng thanh toán mà payment gateway hỗ trợ cho CellphoneS.</summary>
public static class PaymentProviders
{
    public const string PayOs = "PAYOS";
    public const string VnPay = "VNPAY";

    public static readonly string[] Online = { PayOs, VnPay };

    /// <summary>Tên hiển thị ghi vào <c>PaymentTransaction.Provider</c>.</summary>
    public static string DisplayName(string provider) => provider == VnPay ? "VNPay" : "PayOS";
}
