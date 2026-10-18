using cellphones_backend.Data;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminTransactionServiceImpl : IAdminTransactionService
{
    private readonly ApplicationDbContext _context;

    public AdminTransactionServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminTransactionView>> GetTransactions(AdminTransactionFilter filter)
    {
        var query = _context.PaymentTransactions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Provider))
        {
            var provider = filter.Provider.Trim().ToUpperInvariant();
            if (!PaymentProviders.Online.Contains(provider))
                throw new BadRequestException("provider phải là PAYOS hoặc VNPAY.");
            var stored = PaymentProviders.DisplayName(provider);
            query = query.Where(t => t.Provider == stored);
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim().ToLowerInvariant();
            query = query.Where(t => t.Status == status);
        }
        if (filter.OrderId.HasValue)
            query = query.Where(t => t.OrderId == filter.OrderId.Value);
        var (start, end) = AdminDateRange.ToUtcRange(filter.From, filter.To);
        if (start.HasValue) query = query.Where(t => t.CreateDate >= start.Value);
        if (end.HasValue) query = query.Where(t => t.CreateDate < end.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(t => EF.Functions.ILike(t.Reference!, pattern)
                || EF.Functions.ILike(t.PaymentLinkId!, pattern)
                || EF.Functions.ILike(t.Order!.CreateUser!.Email!, pattern));
        }

        var ascending = string.Equals(filter.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        // sortBy chỉ nhận whitelist; Id là khoá phụ để phân trang ổn định.
        var sorted = (filter.SortBy?.ToLowerInvariant(), ascending) switch
        {
            ("amount", true) => query.OrderBy(t => t.Amount).ThenBy(t => t.Id),
            ("amount", false) => query.OrderByDescending(t => t.Amount).ThenByDescending(t => t.Id),
            (_, true) => query.OrderBy(t => t.CreateDate).ThenBy(t => t.Id),
            (_, false) => query.OrderByDescending(t => t.CreateDate).ThenByDescending(t => t.Id)
        };

        return await sorted
            .Select(t => new AdminTransactionView(
                t.Id, t.OrderId, t.Amount, t.Provider, t.Reference, t.Status,
                t.TransactionDate, t.CreateDate, t.Order!.CreateUser!.Email))
            .ToPagedResultAsync(filter.PageIndex, filter.PageSize);
    }
}
