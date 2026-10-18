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
    /// <summary>
    /// Thống kê cho dashboard. Doanh thu chỉ tính đơn Paid/Shipping/Completed; from/to (yyyy-MM-dd) theo giờ Việt Nam,
    /// mặc định 30 ngày gần nhất, tối đa 366 ngày; kết quả cache 5 phút (docs/ADMIN_API_GUIDE.md mục 5).
    /// </summary>
    [Route("api/admin/stats")]
    [ApiController]
    [RequirePermission(Permissions.StatsView)]
    public class AdminStatisticsController : BaseController
    {
        private readonly IAdminStatisticsService _statisticsService;

        public AdminStatisticsController(IAdminStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        // GET: api/admin/stats/overview?from=&to=
        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<StatsOverviewView>>> GetOverview([FromQuery] StatsRangeQuery query)
            => Ok(new ApiResponse<StatsOverviewView>("Success", await _statisticsService.GetOverview(query)));

        // GET: api/admin/stats/revenue?from=&to=&groupBy=day|week|month
        [HttpGet("revenue")]
        public async Task<ActionResult<ApiResponse<List<RevenuePointView>>>> GetRevenue([FromQuery] StatsPeriodQuery query)
            => Ok(new ApiResponse<List<RevenuePointView>>("Success", await _statisticsService.GetRevenue(query)));

        // GET: api/admin/stats/orders-by-status?from=&to=
        [HttpGet("orders-by-status")]
        public async Task<ActionResult<ApiResponse<List<OrderStatusCountView>>>> GetOrdersByStatus([FromQuery] StatsRangeQuery query)
            => Ok(new ApiResponse<List<OrderStatusCountView>>("Success", await _statisticsService.GetOrdersByStatus(query)));

        // GET: api/admin/stats/top-products?from=&to=&limit=10&orderBy=quantity|revenue
        [HttpGet("top-products")]
        public async Task<ActionResult<ApiResponse<List<TopProductView>>>> GetTopProducts([FromQuery] TopProductsQuery query)
            => Ok(new ApiResponse<List<TopProductView>>("Success", await _statisticsService.GetTopProducts(query)));

        // GET: api/admin/stats/revenue-by-category?from=&to=
        [HttpGet("revenue-by-category")]
        public async Task<ActionResult<ApiResponse<List<CategoryRevenueView>>>> GetRevenueByCategory([FromQuery] StatsRangeQuery query)
            => Ok(new ApiResponse<List<CategoryRevenueView>>("Success", await _statisticsService.GetRevenueByCategory(query)));

        // GET: api/admin/stats/payment-providers?from=&to=
        [HttpGet("payment-providers")]
        public async Task<ActionResult<ApiResponse<List<PaymentProviderView>>>> GetPaymentProviders([FromQuery] StatsRangeQuery query)
            => Ok(new ApiResponse<List<PaymentProviderView>>("Success", await _statisticsService.GetPaymentProviders(query)));

        // GET: api/admin/stats/order-types?from=&to=
        [HttpGet("order-types")]
        public async Task<ActionResult<ApiResponse<List<OrderTypeView>>>> GetOrderTypes([FromQuery] StatsRangeQuery query)
            => Ok(new ApiResponse<List<OrderTypeView>>("Success", await _statisticsService.GetOrderTypes(query)));

        // GET: api/admin/stats/new-users?from=&to=&groupBy=day|week|month
        [HttpGet("new-users")]
        public async Task<ActionResult<ApiResponse<List<NewUsersPointView>>>> GetNewUsers([FromQuery] StatsPeriodQuery query)
            => Ok(new ApiResponse<List<NewUsersPointView>>("Success", await _statisticsService.GetNewUsers(query)));

        // GET: api/admin/stats/low-stock?threshold=5&limit=20
        [HttpGet("low-stock")]
        public async Task<ActionResult<ApiResponse<List<LowStockView>>>> GetLowStock([FromQuery] LowStockQuery query)
            => Ok(new ApiResponse<List<LowStockView>>("Success", await _statisticsService.GetLowStock(query)));

        // GET: api/admin/stats/reviews?from=&to=
        [HttpGet("reviews")]
        public async Task<ActionResult<ApiResponse<StatsReviewsView>>> GetReviews([FromQuery] StatsRangeQuery query)
            => Ok(new ApiResponse<StatsReviewsView>("Success", await _statisticsService.GetReviews(query)));
    }
}
