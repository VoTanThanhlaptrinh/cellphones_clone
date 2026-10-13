using cellphones_backend.DTOs.Requests;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cellphones_backend.Services
{
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    public interface IReviewService
    {
        Task<ServiceResult<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request, string userId);
        Task<ServiceResult<List<ReviewResponse>>> GetReviewsByProductIdAsync(long productId);
        Task<ServiceResult<double>> GetAverageRatingAsync(long productId);
    }
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
