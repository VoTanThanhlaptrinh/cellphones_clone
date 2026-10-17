using cellphones_backend.Data;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

/// <summary>
/// Bổ sung dữ liệu thật cho thẻ sản phẩm (tồn kho, đánh giá, thông số nổi bật)
/// bằng vài truy vấn gộp theo danh sách id, tránh N+1.
/// </summary>
public static class ProductCardEnricher
{
    private const int MaxHighlights = 3;
    private const int MaxHighlightLength = 18;

    // Thứ tự ưu tiên khi chọn thông số nổi bật. Exact = so khớp nguyên tên, ngược lại là "chứa".
    private static readonly (string Key, bool Exact)[] HighlightRules =
    {
        ("kích thước màn hình", false),
        ("dung lượng ram", false),
        ("ram", true),
        ("bộ nhớ trong", false),
        ("dung lượng lưu trữ", false),
        ("chipset", false),
        ("loại cpu", false),
        ("dung lượng pin", false),
        ("pin", true),
    };

    private static readonly string[] NoDataValues = { "không công bố", "đang cập nhật", "không có", "không" };

    public static async Task<List<ProductView>> EnrichAsync(ApplicationDbContext context, IEnumerable<ProductView> products)
    {
        var list = products.ToList();
        var ids = list.Where(p => p.Id.HasValue).Select(p => p.Id!.Value).Distinct().ToList();
        if (ids.Count == 0) return list;

        var stockRows = await context.Stores
            .Where(s => ids.Contains(s.ProductId))
            .GroupBy(s => new { s.ProductId, s.ColorId })
            .Select(g => new { g.Key.ProductId, g.Key.ColorId, Quantity = g.Sum(s => s.Quantity) })
            .ToListAsync();
        var stock = stockRows
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));
        // Màu mặc định để thêm nhanh vào giỏ từ thẻ sản phẩm: màu còn nhiều hàng nhất
        var defaultColor = stockRows
            .Where(r => r.Quantity > 0)
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.Quantity).First().ColorId);

        var reviews = await context.Reviews
            .Where(r => ids.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Average = g.Average(r => r.Rating), Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId);

        var details = await (
            from ps in context.ProductSpecifications
            join d in context.SpecificationDetails on ps.SpecificationId equals d.SpecificationId
            where ids.Contains(ps.ProductId) && ps.Status != "deleted" && d.Status != "deleted"
                  && d.Name != null && d.Value != null && d.Value.Length <= MaxHighlightLength
            orderby d.Id
            select new { ps.ProductId, Name = d.Name!, Value = d.Value! }
        ).ToListAsync();
        var detailsByProduct = details.ToLookup(d => d.ProductId);

        return list.Select(p =>
        {
            if (!p.Id.HasValue) return p;
            var id = p.Id.Value;
            reviews.TryGetValue(id, out var review);
            return p with
            {
                Stock = stock.TryGetValue(id, out var qty) ? qty : 0,
                Rating = review is null ? null : Math.Round(review.Average, 1),
                ReviewCount = review?.Count ?? 0,
                Highlights = PickHighlights(detailsByProduct[id].Select(d => (d.Name, d.Value))),
                DefaultColorId = defaultColor.TryGetValue(id, out var colorId) ? colorId : null
            };
        }).ToList();
    }

    public static async Task<List<CategoryView>> EnrichAsync(ApplicationDbContext context, IEnumerable<CategoryView> categories)
    {
        var list = categories.ToList();
        var enriched = (await EnrichAsync(context, list.SelectMany(c => c.Products ?? Enumerable.Empty<ProductView>())))
            .Where(p => p.Id.HasValue)
            .GroupBy(p => p.Id!.Value)
            .ToDictionary(g => g.Key, g => g.First());
        return list.Select(c => c with
        {
            Products = c.Products?.Select(p => p.Id.HasValue && enriched.TryGetValue(p.Id.Value, out var e) ? e : p).ToList()
        }).ToList();
    }

    public static async Task<CategoryDetailView> EnrichAsync(ApplicationDbContext context, CategoryDetailView category)
    {
        var products = category.Products is null ? null : await EnrichAsync(context, category.Products);
        return category with { Products = products };
    }

    private static IReadOnlyList<string> PickHighlights(IEnumerable<(string Name, string Value)> details)
    {
        var candidates = details
            .Select(d => (Name: d.Name.Trim().ToLowerInvariant(), Value: d.Value.Trim()))
            .Where(d => d.Value.Length > 0 && !NoDataValues.Contains(d.Value.ToLowerInvariant()))
            .ToList();

        var picked = new List<string>();
        foreach (var (key, exact) in HighlightRules)
        {
            if (picked.Count >= MaxHighlights) break;
            var match = candidates.FirstOrDefault(c => exact ? c.Name == key : c.Name.Contains(key));
            if (match.Value is not null && !picked.Contains(match.Value))
            {
                picked.Add(match.Value);
            }
        }
        return picked;
    }
}
