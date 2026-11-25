using cellphones_backend.Data;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using cellPhoneS_backend.Utils;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminReviewServiceImpl : IAdminReviewService
{
    private readonly ApplicationDbContext _context;

    public AdminReviewServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminReviewView>> GetReviews(AdminReviewFilter filter)
    {
        var query = _context.Reviews.AsNoTracking().AsQueryable();

        if (filter.ProductId.HasValue)
            query = query.Where(r => r.ProductId == filter.ProductId.Value);
        if (filter.Rating.HasValue)
            query = query.Where(r => r.Rating == filter.Rating.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim().ToLowerInvariant();
            if (!ReviewStatuses.IsValid(status))
                throw new BadRequestException("status phải là visible hoặc hidden.");
            query = query.Where(r => r.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(r => (r.Comment != null && EF.Functions.ILike(r.Comment, pattern))
                                     || (r.User.Email != null && EF.Functions.ILike(r.User.Email, pattern))
                                     || (r.User.UserName != null && EF.Functions.ILike(r.User.UserName, pattern))
                                     || (r.Product.Name != null && EF.Functions.ILike(r.Product.Name, pattern)));
        }
        // from/to là ngày theo giờ Việt Nam, đổi sang khoảng UTC [from 00:00+07, to+1 00:00+07)
        if (filter.From.HasValue)
        {
            var fromUtc = VietnamTime.ToUtc(filter.From.Value);
            query = query.Where(r => r.CreatedDate >= fromUtc);
        }
        if (filter.To.HasValue)
        {
            var toUtc = VietnamTime.ToUtc(filter.To.Value.AddDays(1));
            query = query.Where(r => r.CreatedDate < toUtc);
        }

        var ascending = string.Equals(filter.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        // sortBy chỉ nhận whitelist; Id làm khoá phụ để phân trang ổn định.
        var sorted = (filter.SortBy?.ToLowerInvariant(), ascending) switch
        {
            ("rating", true) => query.OrderBy(r => r.Rating).ThenBy(r => r.Id),
            ("rating", false) => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.Id),
            (_, true) => query.OrderBy(r => r.CreatedDate).ThenBy(r => r.Id),
            (_, false) => query.OrderByDescending(r => r.CreatedDate).ThenByDescending(r => r.Id)
        };

        return await sorted
            .Select(r => new AdminReviewView(r.Id, r.ProductId, r.Product.Name, r.UserId, r.User.Email, r.User.UserName,
                r.Rating, r.Comment, r.Status, r.CreatedDate))
            .ToPagedResultAsync(filter.PageIndex, filter.PageSize);
    }

    public async Task UpdateStatus(int reviewId, UpdateReviewStatusRequest request)
    {
        var status = request.Status?.Trim().ToLowerInvariant();
        if (!ReviewStatuses.IsValid(status))
            throw new BadRequestException("status phải là visible hoặc hidden.");

        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId)
            ?? throw new NotFoundException("Không tìm thấy đánh giá.");
        review.Status = status!;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int reviewId)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId)
            ?? throw new NotFoundException("Không tìm thấy đánh giá.");
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }
}
