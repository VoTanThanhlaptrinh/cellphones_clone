namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>
/// Khoảng ngày của API thống kê (giờ Việt Nam, bao gồm cả ngày <c>To</c>). Bỏ trống: <c>To</c> = hôm nay,
/// <c>From</c> = 29 ngày trước <c>To</c> (30 ngày gần nhất). Tối đa 366 ngày.
/// </summary>
public class StatsRangeQuery
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

/// <summary><c>GroupBy</c> = day | week | month.</summary>
public class StatsPeriodQuery : StatsRangeQuery
{
    public string GroupBy { get; set; } = "day";
}

/// <summary><c>OrderBy</c> = quantity | revenue; <c>Limit</c> 1–50.</summary>
public class TopProductsQuery : StatsRangeQuery
{
    public int Limit { get; set; } = 10;
    public string OrderBy { get; set; } = "quantity";
}

/// <summary>Sản phẩm/màu có tổng tồn (mọi kho) ≤ <c>Threshold</c>; <c>Limit</c> 1–100.</summary>
public class LowStockQuery
{
    public int Threshold { get; set; } = 5;
    public int Limit { get; set; } = 20;
}
