namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Các thẻ tổng quan của dashboard. <c>*ChangePct</c> so với kỳ liền trước cùng độ dài; null khi kỳ trước bằng 0.</summary>
public record StatsOverviewView(
    double Revenue,
    int Orders,
    int PaidOrders,
    double AverageOrderValue,
    int NewCustomers,
    double CancelledRate,
    double? RevenueChangePct,
    double? OrdersChangePct,
    double? NewCustomersChangePct);

/// <summary><c>Period</c> là ngày đầu kỳ (yyyy-MM-dd, giờ VN): chính ngày đó / thứ Hai của tuần / mùng 1 của tháng.</summary>
public record RevenuePointView(string Period, double Revenue, int Orders);

public record OrderStatusCountView(string Status, int Count);

public record TopProductView(long ProductId, string? Name, string? ImageUrl, int Quantity, double Revenue);

public record CategoryRevenueView(long CategoryId, string? Name, double Revenue, int Quantity);

public record PaymentProviderView(string Provider, int Count, long Amount);

public record OrderTypeView(string Type, int Count, double Revenue);

public record NewUsersPointView(string Period, int Count);

public record LowStockView(long ProductId, string? Name, long ColorId, string? ColorName, int TotalQuantity);

public record LowestRatedProductView(long ProductId, string? Name, double Average, int Count);

public record StatsReviewsView(
    int Total,
    double Average,
    Dictionary<string, int> Distribution,
    List<LowestRatedProductView> LowestRated);
