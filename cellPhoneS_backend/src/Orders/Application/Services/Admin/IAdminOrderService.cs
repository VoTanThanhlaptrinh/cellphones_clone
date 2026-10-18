using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Đọc và đổi trạng thái đơn hàng cho trang quản trị (module Orders). Luật chuyển trạng thái nằm trên <c>Order</c>.</summary>
public interface IAdminOrderService
{
    Task<PagedResult<AdminOrderView>> GetOrders(AdminOrderFilter filter);
    Task<AdminOrderDetail> GetOrder(long orderId);
    /// <summary>Shipping / Completed. Sai trạng thái hiện tại → <see cref="ConflictException"/>.</summary>
    Task<AdminOrderDetail> UpdateStatus(long orderId, UpdateOrderStatusRequest request, string adminId);
    Task<CancelOrderResult> Cancel(long orderId, string adminId);
    /// <summary>Hỏi lại gateway rồi cập nhật đơn (<c>OrderService.HandlePaymentCallback</c>); trả trạng thái đơn sau đồng bộ.</summary>
    Task<string> SyncPayment(long orderId);
}
