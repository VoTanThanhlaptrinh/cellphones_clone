using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminUserServiceImpl : IAdminUserService
{
    /// <summary>Đơn đã thu tiền; dùng cho TotalSpent.</summary>
    private static readonly string[] PaidStatuses = { OrderStatuses.Paid, OrderStatuses.Shipping, OrderStatuses.Completed };

    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IAdminRoleService _adminRoleService;
    private readonly IJwtBlacklistService _jwtBlacklistService;

    public AdminUserServiceImpl(
        ApplicationDbContext context,
        UserManager<User> userManager,
        IAdminRoleService adminRoleService,
        IJwtBlacklistService jwtBlacklistService)
    {
        _context = context;
        _userManager = userManager;
        _adminRoleService = adminRoleService;
        _jwtBlacklistService = jwtBlacklistService;
    }

    public async Task<PagedResult<AdminUserView>> GetUsers(AdminUserFilter filter)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(u => u.Status != UserStatuses.Deleted);
        }
        else
        {
            var status = filter.Status.Trim().ToLowerInvariant();
            query = query.Where(u => u.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            var normalized = filter.Role.Trim().ToUpperInvariant();
            var roleId = await _context.Roles.Where(r => r.NormalizedName == normalized).Select(r => r.Id).FirstOrDefaultAsync();
            // Role không tồn tại thì không có user nào mang role đó.
            query = roleId == null
                ? query.Where(u => false)
                : query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId));
        }
        var (start, end) = AdminDateRange.ToUtcRange(filter.From, filter.To);
        if (start.HasValue) query = query.Where(u => u.CreateDate >= start.Value);
        if (end.HasValue) query = query.Where(u => u.CreateDate < end.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(u => EF.Functions.ILike(u.Email!, pattern)
                || EF.Functions.ILike(u.Fullname!, pattern)
                || EF.Functions.ILike(u.PhoneNumber!, pattern));
        }

        var ascending = string.Equals(filter.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        // sortBy chỉ nhận whitelist; CreateDate + Id làm khoá phụ để phân trang ổn định.
        var sorted = (filter.SortBy?.ToLowerInvariant(), ascending) switch
        {
            ("email", true) => query.OrderBy(u => u.Email).ThenBy(u => u.Id),
            ("email", false) => query.OrderByDescending(u => u.Email).ThenByDescending(u => u.Id),
            ("fullname", true) => query.OrderBy(u => u.Fullname).ThenBy(u => u.Id),
            ("fullname", false) => query.OrderByDescending(u => u.Fullname).ThenByDescending(u => u.Id),
            ("status", true) => query.OrderBy(u => u.Status).ThenBy(u => u.Id),
            ("status", false) => query.OrderByDescending(u => u.Status).ThenByDescending(u => u.Id),
            (_, true) => query.OrderBy(u => u.CreateDate).ThenBy(u => u.Id),
            (_, false) => query.OrderByDescending(u => u.CreateDate).ThenByDescending(u => u.Id)
        };

        var (pageIndex, pageSize) = PagingExtensions.NormalizePaging(filter.PageIndex, filter.PageSize);
        var total = await sorted.CountAsync();
        var users = await sorted
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new { u.Id, u.Email, u.Fullname, u.PhoneNumber, u.Status, u.CreateDate })
            .ToListAsync();

        // Role nằm ở bảng nối của Identity; lấy một lần cho cả trang thay vì truy vấn từng user.
        var ids = users.Select(u => u.Id).ToList();
        var roles = await _context.UserRoles
            .Where(ur => ids.Contains(ur.UserId))
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .ToListAsync();
        var rolesByUser = roles.ToLookup(r => r.UserId, r => r.Name!);

        var items = users.Select(u => new AdminUserView(
            u.Id, u.Email, u.Fullname, u.PhoneNumber, u.Status,
            rolesByUser[u.Id].OrderBy(n => n).ToList(), u.CreateDate)).ToList();
        return new PagedResult<AdminUserView>(items, total, pageIndex, pageSize);
    }

    public async Task<AdminUserDetail> GetUser(string userId)
    {
        var user = await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Email, u.Fullname, u.PhoneNumber, u.Birth, u.Status, u.CreateDate, u.UpdateDate })
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Không tìm thấy người dùng.");

        var roles = await GetRoleNames(userId);
        var orders = _context.Orders.AsNoTracking().Where(o => o.CreateBy == userId);
        var orderCount = await orders.CountAsync();
        var totalSpent = await orders
            .Where(o => PaidStatuses.Contains(o.Status))
            .Select(o => o.OrderDetails.Sum(d => d.Price * d.Quantity) + (double)o.Fee.FeeDetails.Sum(f => f.Value))
            .SumAsync();
        var recent = await orders
            .OrderByDescending(o => o.CreateDate).ThenByDescending(o => o.Id)
            .Take(5)
            .Select(o => new AdminUserOrderView(
                o.Id, o.CreateDate, o.Status, o.Type,
                o.OrderDetails.Sum(d => d.Price * d.Quantity) + (double)o.Fee.FeeDetails.Sum(f => f.Value)))
            .ToListAsync();

        return new AdminUserDetail
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.Fullname,
            Phone = user.PhoneNumber,
            Birth = user.Birth,
            Status = user.Status,
            Roles = roles,
            CreateDate = user.CreateDate,
            UpdateDate = user.UpdateDate,
            OrderCount = orderCount,
            TotalSpent = totalSpent,
            RecentOrders = recent
        };
    }

    public async Task UpdateStatus(string userId, UpdateUserStatusRequest request, string actorId)
    {
        var status = request.Status.Trim().ToLowerInvariant();
        var user = await FindUser(userId);

        if (status == UserStatuses.Locked && userId == actorId)
            throw new ForbiddenException("Không thể tự khoá tài khoản của mình.");

        var targetIsAdmin = (await GetRoleNames(userId)).Contains(RoleNames.Admin);
        if (targetIsAdmin && !await IsAdmin(actorId))
            throw new ForbiddenException("Chỉ ADMIN mới khoá hoặc mở khoá được tài khoản ADMIN.");
        if (status == UserStatuses.Locked && targetIsAdmin)
            await _adminRoleService.EnsureNotLastAdmin(userId);

        if (user.Status == status) return;

        user.Status = status;
        user.UpdateDate = DateTime.UtcNow;
        user.UpdateBy = actorId;
        EnsureSucceeded(await _userManager.UpdateAsync(user));

        // Login/refresh đã chặn user không active; access token đang sống thì bị từ chối bằng mốc thu hồi.
        if (status == UserStatuses.Locked)
            await _jwtBlacklistService.RevokeUserSessionsAsync(userId);
    }

    public async Task<List<string>> UpdateRoles(string userId, UpdateUserRolesRequest request, string actorId)
    {
        var user = await FindUser(userId);

        // Tài khoản nào cũng giữ role USER: các route /api/orders/*, /api/carts/* cần nó.
        var wanted = request.Roles.Select(r => r.Trim().ToUpperInvariant()).Append(RoleNames.User).ToHashSet();
        var found = await _context.Roles.AsNoTracking().Where(r => wanted.Contains(r.NormalizedName!)).ToListAsync();
        var missing = wanted.Where(w => found.All(r => r.NormalizedName != w)).ToList();
        if (missing.Count > 0)
            throw new BadRequestException("Role không tồn tại: " + string.Join(", ", missing));
        var inactive = found.Where(r => r.Status != "active").Select(r => r.Name).ToList();
        if (inactive.Count > 0)
            throw new BadRequestException("Role không hoạt động: " + string.Join(", ", inactive));

        var current = (await _userManager.GetRolesAsync(user)).Select(r => r.ToUpperInvariant()).ToHashSet();
        var toAdd = found.Where(r => !current.Contains(r.NormalizedName!)).Select(r => r.Name!).ToList();
        var toRemove = (await _userManager.GetRolesAsync(user)).Where(r => !wanted.Contains(r.ToUpperInvariant())).ToList();

        var changesAdmin = toAdd.Contains(RoleNames.Admin) || toRemove.Contains(RoleNames.Admin);
        if (changesAdmin && !await IsAdmin(actorId))
            throw new ForbiddenException("Chỉ ADMIN mới gán hoặc gỡ được role ADMIN.");
        if (toRemove.Contains(RoleNames.Admin))
        {
            if (userId == actorId)
                throw new ForbiddenException("Không thể tự bỏ role ADMIN của mình.");
            await _adminRoleService.EnsureNotLastAdmin(userId);
        }

        if (toAdd.Count > 0 || toRemove.Count > 0)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            if (toAdd.Count > 0) EnsureSucceeded(await _userManager.AddToRolesAsync(user, toAdd));
            if (toRemove.Count > 0) EnsureSucceeded(await _userManager.RemoveFromRolesAsync(user, toRemove));
            user.UpdateDate = DateTime.UtcNow;
            user.UpdateBy = actorId;
            EnsureSucceeded(await _userManager.UpdateAsync(user));
            await transaction.CommitAsync();

            // JWT mang tên role nên đổi role chỉ có hiệu lực ở lần refresh kế tiếp; thu hồi phiên để áp dụng ngay.
            await _jwtBlacklistService.RevokeUserSessionsAsync(userId);
        }

        return await GetRoleNames(userId);
    }

    private async Task<User> FindUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.Status == UserStatuses.Deleted)
            throw new NotFoundException("Không tìm thấy người dùng.");
        return user;
    }

    private async Task<List<string>> GetRoleNames(string userId)
        => (await _context.UserRoles.AsNoTracking().Where(ur => ur.UserId == userId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
            .ToListAsync()).OrderBy(n => n).ToList();

    private async Task<bool> IsAdmin(string userId) => (await GetRoleNames(userId)).Contains(RoleNames.Admin);

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
