using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>Đơn hàng cho trang quản trị: danh sách/chi tiết (order.read), đổi trạng thái và đồng bộ thanh toán (order.update-status), huỷ (order.cancel).</summary>
    [Route("api/admin/orders")]
    [ApiController]
    [RequirePermission(Permissions.OrderRead)]
    public class AdminOrderController : BaseController
    {
        private readonly IAdminOrderService _adminOrderService;

        public AdminOrderController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        // GET: api/admin/orders?status=Paid&type=delivery&from=2026-10-01&to=2026-10-31&search=abc&sortBy=totalAmount&sortDir=desc
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminOrderView>>>> GetOrders([FromQuery] AdminOrderFilter filter)
        {
            var result = await _adminOrderService.GetOrders(filter);
            return Ok(new ApiResponse<PagedResult<AdminOrderView>>("Success", result));
        }

        // GET: api/admin/orders/{orderId}
        [HttpGet("{orderId:long}")]
        public async Task<ActionResult<ApiResponse<AdminOrderDetail>>> GetOrder(long orderId)
        {
            var result = await _adminOrderService.GetOrder(orderId);
            return Ok(new ApiResponse<AdminOrderDetail>("Success", result));
        }

        // PATCH: api/admin/orders/{orderId}/status
        [HttpPatch("{orderId:long}/status")]
        [RequirePermission(Permissions.OrderUpdateStatus)]
        public async Task<ActionResult<ApiResponse<AdminOrderDetail>>> UpdateStatus(long orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            var result = await _adminOrderService.UpdateStatus(orderId, request, CurrentUserId);
            return Ok(new ApiResponse<AdminOrderDetail>("Cập nhật trạng thái đơn hàng thành công", result));
        }

        // POST: api/admin/orders/{orderId}/cancel
        [HttpPost("{orderId:long}/cancel")]
        [RequirePermission(Permissions.OrderCancel)]
        public async Task<ActionResult<ApiResponse<string>>> Cancel(long orderId, [FromBody] CancelOrderRequest? request = null)
        {
            var result = await _adminOrderService.Cancel(orderId, CurrentUserId);
            var message = result.RefundRequired
                ? "Đã huỷ đơn hàng và hoàn tồn kho. Đơn đã thanh toán online: hệ thống chưa hoàn tiền tự động, cần hoàn tiền thủ công."
                : "Đã huỷ đơn hàng và hoàn tồn kho";
            return Ok(new ApiResponse<string>(message, OrderStatuses.Cancelled));
        }

        // POST: api/admin/orders/{orderId}/sync-payment
        [HttpPost("{orderId:long}/sync-payment")]
        [RequirePermission(Permissions.OrderUpdateStatus)]
        public async Task<ActionResult<ApiResponse<string>>> SyncPayment(long orderId)
        {
            var status = await _adminOrderService.SyncPayment(orderId);
            return Ok(new ApiResponse<string>("Đã đồng bộ trạng thái thanh toán", status));
        }
    }
}
