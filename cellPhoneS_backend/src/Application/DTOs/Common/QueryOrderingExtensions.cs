using System.Linq.Expressions;

namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Sắp xếp theo khoá chọn từ whitelist của từng resource (không đưa chuỗi của client vào OrderBy động).</summary>
public static class QueryOrderingExtensions
{
    public static IOrderedQueryable<T> OrderByDirection<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> key, bool descending)
        => descending ? query.OrderByDescending(key) : query.OrderBy(key);

    /// <summary>true khi <c>sortDir</c> là <c>desc</c> (mặc định), false khi <c>asc</c>; giá trị khác -> 400.</summary>
    public static bool IsDescending(string? sortDir)
    {
        if (string.IsNullOrWhiteSpace(sortDir) || sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase)) return true;
        if (sortDir.Equals("asc", StringComparison.OrdinalIgnoreCase)) return false;
        throw new cellPhoneS_backend.Services.BadRequestException("sortDir chỉ nhận asc hoặc desc.");
    }

    public static Exception InvalidSort(string? sortBy, params string[] allowed)
        => new cellPhoneS_backend.Services.BadRequestException(
            $"sortBy '{sortBy}' không hợp lệ. Giá trị cho phép: {string.Join(", ", allowed)}.");
}
