using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>
/// Bộ lọc <c>GET /api/admin/reviews</c>. <c>Status</c> = visible | hidden (bỏ trống = cả hai); <c>Search</c> khớp nội dung,
/// email/tên người viết hoặc tên sản phẩm; <c>SortBy</c> = createdDate (mặc định) | rating.
/// </summary>
public class AdminReviewFilter : AdminListQuery
{
    public long? ProductId { get; set; }
    public int? Rating { get; set; }
}

/// <summary>Body của <c>PATCH /api/admin/reviews/{id}/status</c>: visible | hidden.</summary>
public record UpdateReviewStatusRequest(string Status);
