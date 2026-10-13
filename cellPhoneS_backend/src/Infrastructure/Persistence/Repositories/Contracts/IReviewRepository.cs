using cellphones_backend.Models;

namespace cellphones_backend.Repositories
{
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    public interface IReviewRepository : IRepository<Review>
    {
        Task<System.Collections.Generic.List<Review>> GetReviewsWithUserByProductIdAsync(long productId);
    }
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
