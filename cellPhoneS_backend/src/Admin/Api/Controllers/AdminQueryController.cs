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
    /// Danh sách chỉ-đọc cho trang quản trị (sản phẩm). Người dùng: AdminUserController; đơn hàng: AdminOrderController; giao dịch: AdminTransactionController.
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

        // GET: api/admin/products?pageIndex=1&pageSize=10&search=iphone
        [HttpGet("products")]
        [RequirePermission(Permissions.ProductRead)]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminProductView>>>> GetProducts(
            [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _adminQueryService.GetProducts(pageIndex, pageSize, search);
            return Ok(new ApiResponse<PagedResult<AdminProductView>>("Success", result));
        }
    }
}
