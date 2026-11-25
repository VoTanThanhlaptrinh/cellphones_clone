using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

public interface OrderService
{
    public Task<string> GetOrderByUserId(string userId);
    public Task<string> GetOrderById(string orderId);
    public Task<string> GetOrders(int page, int pageSize);
    /// <summary>Khách huỷ đơn của chính mình: hoàn tồn kho và huỷ link thanh toán còn mở.</summary>
    public Task DeleteOrder(string userId, long orderId);
    /// <summary>Admin huỷ đơn bất kỳ (cùng luật hoàn kho với <see cref="DeleteOrder"/>). Trả trạng thái của đơn trước khi huỷ; Completed/Cancelled → <see cref="ConflictException"/>.</summary>
    Task<string> AdminCancelOrder(long orderId, string adminId);
    public Task Checkout(string userId, long orderId);
    Task MarkOrderPaid(long orderId, PaymentConfirmation confirmation);
    Task<OnlinePaymentView> StartOnlinePayment(string userId, long orderId, string provider);
    Task<OnlinePaymentView> GetOnlinePayment(string userId, long orderId);
    Task SyncPendingOnlinePayments();
    /// <summary>Gateway báo payment của đơn đã có kết quả cuối: hỏi lại gateway (nguồn sự thật) rồi cập nhật đơn. Idempotent.</summary>
    Task HandlePaymentCallback(long orderId);
    Task<string> GetOrderStatus(string userId, long orderId);
    Task<PagedResult<OrderHistoryView>> GetMyOrders(string userId, int pageIndex, int pageSize);
    Task<OrderView> CreatePickupOrder(string userId, PickupOrderRequest payload);
    Task<OrderView> CreateDeliveryOrder(string userId, DeliveryOrderRequest payload);
}
