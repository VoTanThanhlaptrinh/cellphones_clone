using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories
{
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Review>> GetReviewsWithUserByProductIdAsync(long productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && r.Status == ReviewStatuses.Visible)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }
    }
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
