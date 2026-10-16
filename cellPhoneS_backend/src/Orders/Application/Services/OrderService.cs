using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

public interface OrderService
{
    public Task<string> GetOrderByUserId(string userId);
    public Task<string> GetOrderById(string orderId);
    public Task<string> GetOrders(int page, int pageSize);
    public Task DeleteOrder(string userId, long orderId);
    public Task Checkout(string userId, long orderId);
    Task MarkOrderPaid(long orderId, PaymentConfirmation confirmation);
    Task<OnlinePaymentView> StartOnlinePayment(string userId, long orderId, string provider);
    Task<OnlinePaymentView> GetOnlinePayment(string userId, long paymentId);
    Task SyncPendingOnlinePayments();
    Task<string> GetOrderStatus(string userId, long orderId);
    Task<PagedResult<OrderHistoryView>> GetMyOrders(string userId, int pageIndex, int pageSize);
    Task<OrderView> CreatePickupOrder(string userId, PickupOrderRequest payload);
    Task<OrderView> CreateDeliveryOrder(string userId, DeliveryOrderRequest payload);
}
