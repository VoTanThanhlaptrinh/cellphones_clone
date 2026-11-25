using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>
/// Bộ lọc dùng chung cho <c>GET /api/admin/{categories,brands,series,demands}</c> (mỗi resource chỉ dùng tham số liên quan):
/// categories: <c>ParentCategoryId</c> (0 = danh mục gốc) · brands, demands: <c>CategoryId</c> · series: <c>BrandId</c>, <c>CategoryId</c> (của thương hiệu).
/// <c>Status</c> bỏ trống = active + inactive, <c>deleted</c> = thùng rác.
/// <c>SortBy</c>: createDate (mặc định), updateDate, name, id. <c>From/To</c> không dùng.
/// </summary>
public class AdminCatalogFilter : AdminListQuery
{
    public long? ParentCategoryId { get; set; }
    public long? CategoryId { get; set; }
    public long? BrandId { get; set; }
}
