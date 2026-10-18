using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>
/// Thống kê cho dashboard admin (chỉ đọc). Định nghĩa doanh thu/múi giờ/kỳ so sánh xem docs/ADMIN_API_GUIDE.md mục 5.1.
/// Tham số sai (from &gt; to, quá 366 ngày, groupBy ngoài whitelist…) → <see cref="BadRequestException"/>.
/// Kết quả được cache 5 phút theo endpoint + tham số.
/// </summary>
public interface IAdminStatisticsService
{
    Task<StatsOverviewView> GetOverview(StatsRangeQuery query);
    Task<List<RevenuePointView>> GetRevenue(StatsPeriodQuery query);
    Task<List<OrderStatusCountView>> GetOrdersByStatus(StatsRangeQuery query);
    Task<List<TopProductView>> GetTopProducts(TopProductsQuery query);
    Task<List<CategoryRevenueView>> GetRevenueByCategory(StatsRangeQuery query);
    Task<List<PaymentProviderView>> GetPaymentProviders(StatsRangeQuery query);
    Task<List<OrderTypeView>> GetOrderTypes(StatsRangeQuery query);
    Task<List<NewUsersPointView>> GetNewUsers(StatsPeriodQuery query);
    Task<List<LowStockView>> GetLowStock(LowStockQuery query);
    Task<StatsReviewsView> GetReviews(StatsRangeQuery query);
}
