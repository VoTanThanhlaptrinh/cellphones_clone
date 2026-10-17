using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AuditLogServiceImpl : IAuditLogService
{
    private static readonly TimeZoneInfo VnTz = FindVnTimeZone();
    private readonly ApplicationDbContext _context;

    public AuditLogServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }

    private static TimeZoneInfo FindVnTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }

    public async Task Write(AuditLog entry)
    {
        entry.UserEmail ??= await _context.Users.AsNoTracking()
            .Where(u => u.Id == entry.UserId).Select(u => u.Email).FirstOrDefaultAsync();
        entry.CreateDate = DateTime.UtcNow;
        _context.AuditLogs.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<AuditLogView>> GetAuditLogs(AuditLogFilter filter)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.UserId))
            query = query.Where(a => a.UserId == filter.UserId);
        if (!string.IsNullOrWhiteSpace(filter.Resource))
        {
            var resource = filter.Resource.Trim().ToLower();
            query = query.Where(a => a.Resource == resource);
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (!int.TryParse(filter.Status, out var statusCode))
                throw new BadRequestException("status phải là mã HTTP, ví dụ 200 hoặc 403.");
            query = query.Where(a => a.StatusCode == statusCode);
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(a => EF.Functions.ILike(a.Path, pattern) || EF.Functions.ILike(a.UserEmail!, pattern));
        }
        // from/to là ngày theo giờ Việt Nam: [from 00:00+07, to+1 00:00+07)
        if (filter.From.HasValue)
        {
            var fromUtc = ToUtc(filter.From.Value);
            query = query.Where(a => a.CreateDate >= fromUtc);
        }
        if (filter.To.HasValue)
        {
            var toUtc = ToUtc(filter.To.Value.AddDays(1));
            query = query.Where(a => a.CreateDate < toUtc);
        }

        var ascending = string.Equals(filter.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        // sortBy chỉ nhận whitelist; mặc định createDate. Id làm khoá phụ để phân trang ổn định.
        var sorted = (filter.SortBy?.ToLowerInvariant(), ascending) switch
        {
            ("statuscode", true) => query.OrderBy(a => a.StatusCode).ThenBy(a => a.Id),
            ("statuscode", false) => query.OrderByDescending(a => a.StatusCode).ThenByDescending(a => a.Id),
            (_, true) => query.OrderBy(a => a.CreateDate).ThenBy(a => a.Id),
            (_, false) => query.OrderByDescending(a => a.CreateDate).ThenByDescending(a => a.Id)
        };

        return await sorted
            .Select(a => new AuditLogView(a.Id, a.UserId, a.UserEmail, a.Method, a.Path, a.Resource, a.ResourceId,
                a.StatusCode, a.Payload, a.Ip, a.CreateDate))
            .ToPagedResultAsync(filter.PageIndex, filter.PageSize);
    }

    private static DateTime ToUtc(DateOnly day)
        => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), VnTz), DateTimeKind.Utc);
}
