using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>
/// Bộ lọc <c>GET /api/admin/products</c>. <c>Status</c> bỏ trống = active + inactive, <c>deleted</c> = thùng rác.
/// <c>SortBy</c>: createDate (mặc định), updateDate, name, salePrice, basePrice, stock, id. <c>From/To</c> không dùng.
/// </summary>
public class AdminProductFilter : AdminListQuery
{
    public long? CategoryId { get; set; }
    /// <summary>Sản phẩm không gắn trực tiếp với thương hiệu: lọc theo danh mục của thương hiệu (giống storefront).</summary>
    public long? BrandId { get; set; }
    /// <summary>Ngưỡng tồn thấp: chỉ lấy sản phẩm có tổng tồn &lt;= giá trị này (0 = hết hàng).</summary>
    public int? LowStock { get; set; }
}

public record UpdateProductStatusRequest(string Status);
