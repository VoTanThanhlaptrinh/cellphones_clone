using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Kiểm duyệt đánh giá sản phẩm cho admin (module Products). Đọc/ghi thẳng <c>ApplicationDbContext</c>, không đi qua <c>IReviewService</c> của storefront.</summary>
public interface IAdminReviewService
{
    Task<PagedResult<AdminReviewView>> GetReviews(AdminReviewFilter filter);
    /// <summary>Đặt trạng thái visible/hidden (idempotent); id không có → <see cref="NotFoundException"/>.</summary>
    Task UpdateStatus(int reviewId, UpdateReviewStatusRequest request);
    Task Delete(int reviewId);
}
