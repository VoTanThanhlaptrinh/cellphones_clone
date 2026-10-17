namespace cellPhoneS_backend.Services.Interface;

/// <summary>Ném <see cref="PaymentGatewayException"/> khi gateway từ chối hoặc không liên lạc được.</summary>
public interface IPaymentGatewayClient
{
    /// <summary>Cùng <paramref name="idempotencyKey"/> và cùng nội dung thì gateway trả lại payment cũ.</summary>
    Task<GatewayPayment> Checkout(long orderId, long amount, string provider, string idempotencyKey);

    /// <summary>Null nếu gateway chưa có payment nào cho đơn.</summary>
    Task<GatewayPayment?> GetStatus(long orderId);

    Task<GatewayPayment> Cancel(long orderId, string? reason);
}
