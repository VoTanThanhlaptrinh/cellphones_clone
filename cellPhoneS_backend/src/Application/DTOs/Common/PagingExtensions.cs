using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.DTOs.Responses;

public static class PagingExtensions
{
    public const int MaxPageSize = 100;

    public static (int pageIndex, int pageSize) NormalizePaging(int pageIndex, int pageSize)
        => (Math.Max(pageIndex, 1), Math.Clamp(pageSize, 1, MaxPageSize));

    /// <summary>Đếm + lấy một trang. <paramref name="query"/> phải đã được sắp xếp.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageIndex, int pageSize)
    {
        (pageIndex, pageSize) = NormalizePaging(pageIndex, pageSize);
        var total = await query.CountAsync();
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<T>(items, total, pageIndex, pageSize);
    }
}
