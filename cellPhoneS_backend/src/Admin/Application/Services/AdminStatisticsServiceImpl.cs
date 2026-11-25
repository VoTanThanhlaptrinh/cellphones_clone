using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using cellPhoneS_backend.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace cellPhoneS_backend.Services.Implement;

/// <summary>Kết quả thô của truy vấn SQL theo kỳ (không phải entity).</summary>
internal sealed class RevenuePeriodRow
{
    public DateTime Period { get; set; }
    public double Revenue { get; set; }
    public int Orders { get; set; }
}

internal sealed class CountPeriodRow
{
    public DateTime Period { get; set; }
    public int Count { get; set; }
}

public class AdminStatisticsServiceImpl : IAdminStatisticsService
{
    private const int DefaultDays = 30;
    private const int MaxDays = 366;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public AdminStatisticsServiceImpl(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    #region Khoảng thời gian, cache

    /// <summary>Áp mặc định (30 ngày gần nhất theo giờ VN) và kiểm tra from ≤ to, tối đa 366 ngày.</summary>
    private static (DateOnly from, DateOnly to) ResolveRange(StatsRangeQuery query)
    {
        var to = query.To ?? VietnamTime.Today();
        var from = query.From ?? to.AddDays(-(DefaultDays - 1));
        if (from > to) throw new BadRequestException("from phải nhỏ hơn hoặc bằng to.");
        if (to.DayNumber - from.DayNumber + 1 > MaxDays) throw new BadRequestException($"Khoảng thời gian tối đa {MaxDays} ngày.");
        return (from, to);
    }

    /// <summary>Đổi [from, to] (ngày giờ VN, gồm cả ngày to) sang [startUtc, endUtc).</summary>
    private static (DateTime startUtc, DateTime endUtc) ToUtcRange(DateOnly from, DateOnly to)
        => (VietnamTime.ToUtc(from), VietnamTime.ToUtc(to.AddDays(1)));

    private static string NormalizeGroupBy(string? groupBy)
    {
        var value = groupBy?.Trim().ToLowerInvariant();
        return value is "day" or "week" or "month"
            ? value
            : throw new BadRequestException("groupBy phải là day, week hoặc month.");
    }

    private async Task<T> Cached<T>(string key, Func<Task<T>> factory)
        => (await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await factory();
        }))!;

    private static double? ChangePct(double current, double previous)
        => previous == 0 ? null : Math.Round((current - previous) / previous * 100, 1);

    /// <summary>Ngày đầu của kỳ chứa <paramref name="day"/> (khớp <c>date_trunc</c> của Postgres: tuần bắt đầu thứ Hai).</summary>
    private static DateOnly AlignToPeriod(DateOnly day, string groupBy) => groupBy switch
    {
        "week" => day.AddDays(-(((int)day.DayOfWeek + 6) % 7)),
        "month" => new DateOnly(day.Year, day.Month, 1),
        _ => day
    };

    private static DateOnly NextPeriod(DateOnly period, string groupBy) => groupBy switch
    {
        "week" => period.AddDays(7),
        "month" => period.AddMonths(1),
        _ => period.AddDays(1)
    };

    private static string Label(DateOnly period) => period.ToString("yyyy-MM-dd");

    #endregion

    #region Truy vấn dùng chung

    /// <summary>Đơn tính doanh thu (Paid/Shipping/Completed) tạo trong [startUtc, endUtc).</summary>
    private IQueryable<Order> RevenueOrders(DateTime startUtc, DateTime endUtc)
        => _context.Orders.AsNoTracking()
            .Where(o => o.CreateDate >= startUtc && o.CreateDate < endUtc && OrderStatuses.Revenue.Contains(o.Status));

    /// <summary>Doanh thu = Σ giá × số lượng + Σ phí ship của các đơn tính doanh thu (giống <c>Order.GetTotalAmount()</c>).</summary>
    private async Task<(double revenue, int paidOrders)> SumRevenue(DateTime startUtc, DateTime endUtc)
    {
        var orders = RevenueOrders(startUtc, endUtc);
        var paidOrders = await orders.CountAsync();
        var goods = await orders.SelectMany(o => o.OrderDetails).SumAsync(d => (double?)(d.Price * d.Quantity)) ?? 0;
        var shipping = await orders.SelectMany(o => o.Fee.FeeDetails).SumAsync(f => (long?)f.Value) ?? 0;
        return (goods + shipping, paidOrders);
    }

    #endregion

    public Task<StatsOverviewView> GetOverview(StatsRangeQuery query)
    {
        var (from, to) = ResolveRange(query);
        return Cached($"stats:overview:{from:yyyyMMdd}:{to:yyyyMMdd}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            var days = to.DayNumber - from.DayNumber + 1;
            var (prevStart, prevEnd) = ToUtcRange(from.AddDays(-days), from.AddDays(-1));

            // Các truy vấn chạy tuần tự: DbContext không an toàn khi chạy song song.
            var cur = await SumRevenue(start, end);
            var prev = await SumRevenue(prevStart, prevEnd);
            var total = await _context.Orders.CountAsync(o => o.CreateDate >= start && o.CreateDate < end);
            var prevTotal = await _context.Orders.CountAsync(o => o.CreateDate >= prevStart && o.CreateDate < prevEnd);
            var cancelled = await _context.Orders.CountAsync(o => o.CreateDate >= start && o.CreateDate < end && o.Status == OrderStatuses.Cancelled);
            var newCustomers = await _context.Users.CountAsync(u => u.CreateDate >= start && u.CreateDate < end);
            var prevNewCustomers = await _context.Users.CountAsync(u => u.CreateDate >= prevStart && u.CreateDate < prevEnd);

            return new StatsOverviewView(
                Revenue: Math.Round(cur.revenue),
                Orders: total,
                PaidOrders: cur.paidOrders,
                AverageOrderValue: cur.paidOrders == 0 ? 0 : Math.Round(cur.revenue / cur.paidOrders),
                NewCustomers: newCustomers,
                CancelledRate: total == 0 ? 0 : Math.Round((double)cancelled / total, 4),
                RevenueChangePct: ChangePct(cur.revenue, prev.revenue),
                OrdersChangePct: ChangePct(total, prevTotal),
                NewCustomersChangePct: ChangePct(newCustomers, prevNewCustomers));
        });
    }

    public Task<List<RevenuePointView>> GetRevenue(StatsPeriodQuery query)
    {
        var (from, to) = ResolveRange(query);
        var groupBy = NormalizeGroupBy(query.GroupBy);
        return Cached($"stats:revenue:{from:yyyyMMdd}:{to:yyyyMMdd}:{groupBy}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            var statuses = OrderStatuses.Revenue;

            // EF Core không dịch được GroupBy theo date_trunc + múi giờ nên dùng SQL thuần. groupBy đi vào SQL dưới dạng
            // tham số (và đã qua whitelist). OrderDetails."OrderId" là FK ẩn do EF sinh từ Order.OrderDetails (nullable).
            var rows = await _context.Database.SqlQuery<RevenuePeriodRow>($"""
                SELECT date_trunc({groupBy}, o."CreateDate" AT TIME ZONE 'Asia/Ho_Chi_Minh') AS "Period",
                       (COALESCE(SUM(d.goods), 0) + COALESCE(SUM(f.shipping), 0))::float8 AS "Revenue",
                       COUNT(*)::int AS "Orders"
                FROM "Orders" o
                LEFT JOIN (SELECT "OrderId", SUM("Price" * "Quantity") AS goods
                           FROM "OrderDetails" GROUP BY "OrderId") d ON d."OrderId" = o."Id"
                LEFT JOIN (SELECT "FeeId", SUM("Value")::float8 AS shipping
                           FROM "FeeDetails" GROUP BY "FeeId") f ON f."FeeId" = o."FeeId"
                WHERE o."CreateDate" >= {start} AND o."CreateDate" < {end}
                  AND o."Status" = ANY({statuses})
                GROUP BY 1
                ORDER BY 1
                """).ToListAsync();

            var byPeriod = rows.ToDictionary(r => DateOnly.FromDateTime(r.Period));
            var result = new List<RevenuePointView>();
            // Lấp kỳ trống để biểu đồ liền mạch.
            for (var p = AlignToPeriod(from, groupBy); p <= to; p = NextPeriod(p, groupBy))
            {
                byPeriod.TryGetValue(p, out var row);
                result.Add(new RevenuePointView(Label(p), Math.Round(row?.Revenue ?? 0), row?.Orders ?? 0));
            }
            return result;
        });
    }

    public Task<List<OrderStatusCountView>> GetOrdersByStatus(StatsRangeQuery query)
    {
        var (from, to) = ResolveRange(query);
        return Cached($"stats:orders-by-status:{from:yyyyMMdd}:{to:yyyyMMdd}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            var rows = await _context.Orders.AsNoTracking()
                .Where(o => o.CreateDate >= start && o.CreateDate < end)
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            var counts = rows.ToDictionary(r => r.Status, r => r.Count);

            // Luôn trả đủ các trạng thái đã biết (theo vòng đời), kể cả 0; trạng thái lạ (nếu có) xếp sau.
            var known = new[] { OrderStatuses.Pending, OrderStatuses.Paid, OrderStatuses.Shipping, OrderStatuses.Completed, OrderStatuses.Cancelled };
            return known.Select(s => new OrderStatusCountView(s, counts.GetValueOrDefault(s)))
                .Concat(rows.Where(r => !known.Contains(r.Status)).OrderBy(r => r.Status).Select(r => new OrderStatusCountView(r.Status, r.Count)))
                .ToList();
        });
    }

    public Task<List<TopProductView>> GetTopProducts(TopProductsQuery query)
    {
        var (from, to) = ResolveRange(query);
        var orderBy = query.OrderBy?.Trim().ToLowerInvariant();
        if (orderBy is not ("quantity" or "revenue")) throw new BadRequestException("orderBy phải là quantity hoặc revenue.");
        var limit = Math.Clamp(query.Limit, 1, 50);
        return Cached($"stats:top-products:{from:yyyyMMdd}:{to:yyyyMMdd}:{orderBy}:{limit}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            var grouped = RevenueOrders(start, end)
                .SelectMany(o => o.OrderDetails)
                .GroupBy(d => new { d.ProductOrderDetailId, d.Product!.Name, d.Product.ImageUrl })
                .Select(g => new
                {
                    ProductId = g.Key.ProductOrderDetailId,
                    g.Key.Name,
                    g.Key.ImageUrl,
                    Quantity = g.Sum(d => d.Quantity),
                    Revenue = g.Sum(d => d.Price * d.Quantity)
                });
            var sorted = orderBy == "revenue"
                ? grouped.OrderByDescending(x => x.Revenue).ThenByDescending(x => x.Quantity)
                : grouped.OrderByDescending(x => x.Quantity).ThenByDescending(x => x.Revenue);
            var rows = await sorted.ThenBy(x => x.ProductId).Take(limit).ToListAsync();
            return rows.Select(x => new TopProductView(x.ProductId, x.Name, x.ImageUrl, x.Quantity, Math.Round(x.Revenue))).ToList();
        });
    }

    public Task<List<CategoryRevenueView>> GetRevenueByCategory(StatsRangeQuery query)
    {
        var (from, to) = ResolveRange(query);
        return Cached($"stats:revenue-by-category:{from:yyyyMMdd}:{to:yyyyMMdd}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            // Sản phẩm thuộc nhiều danh mục được tính cho từng danh mục; doanh thu hàng hoá, không gồm phí ship.
            var rows = await (
                from o in RevenueOrders(start, end)
                from d in o.OrderDetails
                join cp in _context.CategoryProducts on d.ProductOrderDetailId equals cp.ProductId
                where cp.Status != "deleted" && cp.Category!.Status != "deleted"
                group d by new { cp.CategoryId, cp.Category!.Name } into g
                select new
                {
                    g.Key.CategoryId,
                    g.Key.Name,
                    Revenue = g.Sum(d => d.Price * d.Quantity),
                    Quantity = g.Sum(d => d.Quantity)
                })
                .OrderByDescending(x => x.Revenue).ThenBy(x => x.CategoryId)
                .ToListAsync();
            return rows.Select(x => new CategoryRevenueView(x.CategoryId, x.Name, Math.Round(x.Revenue), x.Quantity)).ToList();
        });
    }

    public Task<List<PaymentProviderView>> GetPaymentProviders(StatsRangeQuery query)
    {
        var (from, to) = ResolveRange(query);
        return Cached($"stats:payment-providers:{from:yyyyMMdd}:{to:yyyyMMdd}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            var rows = await _context.PaymentTransactions.AsNoTracking()
                .Where(t => t.Status == PaymentTransactionStatuses.Success && t.CreateDate >= start && t.CreateDate < end)
                .GroupBy(t => t.Provider)
                .Select(g => new { Provider = g.Key, Count = g.Count(), Amount = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Amount).ThenBy(x => x.Provider)
                .ToListAsync();
            return rows.Select(x => new PaymentProviderView(x.Provider, x.Count, x.Amount)).ToList();
        });
    }

    public Task<List<OrderTypeView>> GetOrderTypes(StatsRangeQuery query)
    {
        var (from, to) = ResolveRange(query);
        return Cached($"stats:order-types:{from:yyyyMMdd}:{to:yyyyMMdd}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            var orders = RevenueOrders(start, end);
            var counts = await orders.GroupBy(o => o.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() }).ToListAsync();
            var goods = await orders.SelectMany(o => o.OrderDetails.Select(d => new { o.Type, Amount = d.Price * d.Quantity }))
                .GroupBy(x => x.Type)
                .Select(g => new { Type = g.Key, Sum = g.Sum(x => x.Amount) }).ToListAsync();
            var shipping = await orders.SelectMany(o => o.Fee.FeeDetails.Select(f => new { o.Type, f.Value }))
                .GroupBy(x => x.Type)
                .Select(g => new { Type = g.Key, Sum = g.Sum(x => x.Value) }).ToListAsync();

            // Chỉ tính đơn có doanh thu (cùng định nghĩa với revenue); luôn trả đủ pickup và delivery.
            var types = new[] { OrderTypes.Pickup, OrderTypes.Delivery }
                .Concat(counts.Select(c => c.Type)).Distinct().ToList();
            return types.Select(t => new OrderTypeView(
                t,
                counts.FirstOrDefault(c => c.Type == t)?.Count ?? 0,
                Math.Round((goods.FirstOrDefault(g => g.Type == t)?.Sum ?? 0) + (shipping.FirstOrDefault(s => s.Type == t)?.Sum ?? 0)))).ToList();
        });
    }

    public Task<List<NewUsersPointView>> GetNewUsers(StatsPeriodQuery query)
    {
        var (from, to) = ResolveRange(query);
        var groupBy = NormalizeGroupBy(query.GroupBy);
        return Cached($"stats:new-users:{from:yyyyMMdd}:{to:yyyyMMdd}:{groupBy}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            var rows = await _context.Database.SqlQuery<CountPeriodRow>($"""
                SELECT date_trunc({groupBy}, u."CreateDate" AT TIME ZONE 'Asia/Ho_Chi_Minh') AS "Period",
                       COUNT(*)::int AS "Count"
                FROM "AspNetUsers" u
                WHERE u."CreateDate" >= {start} AND u."CreateDate" < {end}
                GROUP BY 1
                ORDER BY 1
                """).ToListAsync();

            var byPeriod = rows.ToDictionary(r => DateOnly.FromDateTime(r.Period), r => r.Count);
            var result = new List<NewUsersPointView>();
            for (var p = AlignToPeriod(from, groupBy); p <= to; p = NextPeriod(p, groupBy))
                result.Add(new NewUsersPointView(Label(p), byPeriod.GetValueOrDefault(p)));
            return result;
        });
    }

    public Task<List<LowStockView>> GetLowStock(LowStockQuery query)
    {
        var threshold = Math.Max(query.Threshold, 0);
        var limit = Math.Clamp(query.Limit, 1, 100);
        return Cached($"stats:low-stock:{threshold}:{limit}", async () =>
        {
            // Tổng tồn của từng (sản phẩm, màu) trên mọi kho; chỉ sản phẩm đang bán. Cặp chưa từng có dòng tồn kho thì không có mặt ở đây.
            var rows = await _context.Stores.AsNoTracking()
                .Where(s => s.Status != "deleted" && s.Product!.Status == "active")
                .GroupBy(s => new { s.ProductId, ProductName = s.Product!.Name, s.ColorId, ColorName = s.Color!.Name })
                .Select(g => new { g.Key.ProductId, g.Key.ProductName, g.Key.ColorId, g.Key.ColorName, Total = g.Sum(s => s.Quantity) })
                .Where(x => x.Total <= threshold)
                .OrderBy(x => x.Total).ThenBy(x => x.ProductId).ThenBy(x => x.ColorId)
                .Take(limit)
                .ToListAsync();
            return rows.Select(x => new LowStockView(x.ProductId, x.ProductName, x.ColorId, x.ColorName, x.Total)).ToList();
        });
    }

    public Task<StatsReviewsView> GetReviews(StatsRangeQuery query)
    {
        var (from, to) = ResolveRange(query);
        return Cached($"stats:reviews:{from:yyyyMMdd}:{to:yyyyMMdd}", async () =>
        {
            var (start, end) = ToUtcRange(from, to);
            // Chỉ review visible, tính theo ngày viết.
            var reviews = _context.Reviews.AsNoTracking()
                .Where(r => r.Status == ReviewStatuses.Visible && r.CreatedDate >= start && r.CreatedDate < end);

            var byRating = (await reviews.GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() }).ToListAsync())
                .ToDictionary(x => x.Rating, x => x.Count);
            var distribution = Enumerable.Range(1, 5).ToDictionary(r => r.ToString(), r => byRating.GetValueOrDefault(r));
            var total = byRating.Values.Sum();
            var average = total == 0 ? 0 : Math.Round(byRating.Sum(x => (double)x.Key * x.Value) / total, 1);

            var lowest = (await reviews
                .GroupBy(r => new { r.ProductId, r.Product.Name })
                .Select(g => new { g.Key.ProductId, g.Key.Name, Average = g.Average(r => r.Rating), Count = g.Count() })
                .OrderBy(x => x.Average).ThenByDescending(x => x.Count).ThenBy(x => x.ProductId)
                .Take(5)
                .ToListAsync())
                .Select(x => new LowestRatedProductView(x.ProductId, x.Name, Math.Round(x.Average, 1), x.Count))
                .ToList();

            return new StatsReviewsView(total, average, distribution, lowest);
        });
    }
}
