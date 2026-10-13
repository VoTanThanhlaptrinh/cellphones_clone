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
        Task<ReviewResponse> CreateReviewAsync(CreateReviewRequest request, string userId);
        Task<List<ReviewResponse>> GetReviewsByProductIdAsync(long productId);
        Task<double> GetAverageRatingAsync(long productId);
    }
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
