using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>
    /// Danh sách chỉ-đọc cho trang quản trị (người dùng, đơn hàng, giao dịch).
    /// Cổng `admin.access` do RouteConfig chặn; từng action khai báo permission đọc riêng bằng [RequirePermission].
    /// </summary>
    [Route("api/admin")]
    [ApiController]
    public class AdminQueryController : BaseController
    {
        private readonly IAdminQueryService _adminQueryService;

        public AdminQueryController(IAdminQueryService adminQueryService)
        {
            _adminQueryService = adminQueryService;
        }

        // GET: api/admin/users?pageIndex=1&pageSize=10&search=abc
        [HttpGet("users")]
        [RequirePermission(Permissions.UserRead)]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminUserView>>>> GetUsers(
            [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _adminQueryService.GetUsers(pageIndex, pageSize, search);
            return Ok(new ApiResponse<PagedResult<AdminUserView>>("Success", result));
        }

        // GET: api/admin/orders?pageIndex=1&pageSize=10&status=Paid
        [HttpGet("orders")]
        [RequirePermission(Permissions.OrderRead)]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminOrderView>>>> GetOrders(
            [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            var result = await _adminQueryService.GetOrders(pageIndex, pageSize, status);
            return Ok(new ApiResponse<PagedResult<AdminOrderView>>("Success", result));
        }

        // GET: api/admin/orders/{orderId}
        [HttpGet("orders/{orderId}")]
        [RequirePermission(Permissions.OrderRead)]
        public async Task<ActionResult<ApiResponse<AdminOrderView>>> GetOrder(long orderId)
        {
            var result = await _adminQueryService.GetOrder(orderId);
            return Ok(new ApiResponse<AdminOrderView>("Success", result));
        }

        // GET: api/admin/transactions?pageIndex=1&pageSize=10
        [HttpGet("transactions")]
        [RequirePermission(Permissions.TransactionRead)]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminTransactionView>>>> GetTransactions(
            [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _adminQueryService.GetTransactions(pageIndex, pageSize);
            return Ok(new ApiResponse<PagedResult<AdminTransactionView>>("Success", result));
        }
    }
}
