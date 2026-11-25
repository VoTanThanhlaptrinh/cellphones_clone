using System.Security.Claims;
using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminRoleServiceImpl : IAdminRoleService
{
    private static readonly Dictionary<string, string> PermissionDescriptions = new()
    {
        [Permissions.AdminAccess] = "Vào khu quản trị",
        [Permissions.CatalogRead] = "Xem danh mục, thương hiệu, dòng sản phẩm, nhu cầu",
        [Permissions.CatalogWrite] = "Thêm/sửa/xoá danh mục, thương hiệu, dòng sản phẩm, nhu cầu",
        [Permissions.ProductRead] = "Xem sản phẩm",
        [Permissions.ProductWrite] = "Thêm/sửa sản phẩm, thông số, cam kết",
        [Permissions.ProductDelete] = "Xoá/khôi phục sản phẩm",
        [Permissions.MediaWrite] = "Tải lên/xoá ảnh",
        [Permissions.InventoryRead] = "Xem kho và tồn kho",
        [Permissions.InventoryWrite] = "Sửa kho và tồn kho",
        [Permissions.OrderRead] = "Xem đơn hàng",
        [Permissions.OrderUpdateStatus] = "Đổi trạng thái đơn, đồng bộ thanh toán",
        [Permissions.OrderCancel] = "Huỷ đơn hàng",
        [Permissions.TransactionRead] = "Xem giao dịch thanh toán",
        [Permissions.UserRead] = "Xem người dùng",
        [Permissions.UserLock] = "Khoá/mở khoá người dùng",
        [Permissions.UserAssignRole] = "Gán role cho người dùng",
        [Permissions.RoleManage] = "Quản lý role và permission",
        [Permissions.ReviewModerate] = "Kiểm duyệt đánh giá",
        [Permissions.StatsView] = "Xem thống kê",
        [Permissions.AuditRead] = "Xem nhật ký thao tác"
    };

    private readonly ApplicationDbContext _context;
    private readonly RoleManager<Role> _roleManager;
    private readonly IPermissionService _permissionService;

    public AdminRoleServiceImpl(ApplicationDbContext context, RoleManager<Role> roleManager, IPermissionService permissionService)
    {
        _context = context;
        _roleManager = roleManager;
        _permissionService = permissionService;
    }

    public async Task<AdminMeView> GetMe(string userId)
    {
        var user = await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Email, u.Fullname })
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Không tìm thấy tài khoản.");
        var roles = await _context.UserRoles.Where(ur => ur.UserId == userId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
            .ToListAsync();
        var permissions = await _permissionService.GetPermissionsAsync(roles);
        return new AdminMeView(user.Id, user.Email, user.Fullname,
            roles.OrderBy(r => r).ToList(), permissions.OrderBy(p => p).ToList());
    }

    public List<PermissionView> GetPermissions()
        => Permissions.All
            .Select(p => new PermissionView(p, p.Split('.')[0], PermissionDescriptions.GetValueOrDefault(p, p)))
            .ToList();

    public async Task<List<RoleView>> GetRoles()
    {
        var roles = await _context.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
        var claims = await _context.RoleClaims.AsNoTracking()
            .Where(rc => rc.ClaimType == Permissions.ClaimType)
            .ToListAsync();
        var userCounts = await _context.UserRoles.GroupBy(ur => ur.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count);
        var permissionsByRole = claims.ToLookup(c => c.RoleId, c => c.ClaimValue!);

        return roles.Select(r => ToView(r, userCounts.GetValueOrDefault(r.Id), permissionsByRole[r.Id])).ToList();
    }

    public async Task<RoleView> GetRole(string roleId)
    {
        var role = await FindRole(roleId);
        var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        var permissions = await _context.RoleClaims.AsNoTracking()
            .Where(rc => rc.RoleId == role.Id && rc.ClaimType == Permissions.ClaimType)
            .Select(rc => rc.ClaimValue!).ToListAsync();
        return ToView(role, userCount, permissions);
    }

    public async Task<string> CreateRole(CreateRoleRequest request)
    {
        var name = NormalizeName(request.Name);
        var permissions = ValidatePermissions(request.Permissions ?? new List<string>());
        if (await _roleManager.FindByNameAsync(name) != null)
            throw new ConflictException($"Role {name} đã tồn tại.");

        var role = new Role
        {
            Name = name,
            Status = "active",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow
        };
        EnsureSucceeded(await _roleManager.CreateAsync(role));
        foreach (var p in permissions)
            EnsureSucceeded(await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, p)));
        _permissionService.Invalidate(name);
        return role.Id;
    }

    public async Task UpdateRole(string roleId, UpdateRoleRequest request)
    {
        if (roleId != request.Id)
            throw new BadRequestException("Role ID in URL does not match ID in request body");
        var role = await FindRole(roleId);
        var name = NormalizeName(request.Name);
        if (name == role.Name) return;

        if (IsSystem(role.Name))
            throw new ConflictException($"Role hệ thống {role.Name} không thể đổi tên.");
        var duplicate = await _roleManager.FindByNameAsync(name);
        if (duplicate != null && duplicate.Id != role.Id)
            throw new ConflictException($"Role {name} đã tồn tại.");

        var oldName = role.Name!;
        role.Name = name;
        role.UpdateDate = DateTime.UtcNow;
        EnsureSucceeded(await _roleManager.UpdateAsync(role));
        // JWT cũ còn mang tên cũ nên xoá cả hai khoá cache.
        _permissionService.Invalidate(oldName);
        _permissionService.Invalidate(name);
    }

    public async Task UpdateRolePermissions(string roleId, UpdateRolePermissionsRequest request)
    {
        var role = await FindRole(roleId);
        if (role.Name == RoleNames.Admin)
            throw new ForbiddenException("Permission của role ADMIN luôn là toàn bộ danh mục và không thể sửa.");
        if (role.Name == RoleNames.User)
            throw new ForbiddenException("Role USER là role khách hàng, không thể gán permission quản trị.");

        var wanted = ValidatePermissions(request.Permissions);
        var existing = (await _roleManager.GetClaimsAsync(role)).Where(c => c.Type == Permissions.ClaimType).ToList();

        foreach (var claim in existing.Where(c => !wanted.Contains(c.Value)))
            EnsureSucceeded(await _roleManager.RemoveClaimAsync(role, claim));
        var current = existing.Select(c => c.Value).ToHashSet();
        foreach (var p in wanted.Where(p => !current.Contains(p)))
            EnsureSucceeded(await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, p)));

        role.UpdateDate = DateTime.UtcNow;
        await _roleManager.UpdateAsync(role);
        _permissionService.Invalidate(role.Name!);
    }

    public async Task DeleteRole(string roleId)
    {
        var role = await FindRole(roleId);
        if (IsSystem(role.Name))
            throw new ConflictException($"Role hệ thống {role.Name} không thể xoá.");
        if (await _context.UserRoles.AnyAsync(ur => ur.RoleId == role.Id))
            throw new ConflictException($"Role {role.Name} vẫn còn người dùng, hãy gỡ role khỏi họ trước.");

        EnsureSucceeded(await _roleManager.DeleteAsync(role));
        _permissionService.Invalidate(role.Name!);
    }

    public async Task EnsureNotLastAdmin(string userId)
    {
        var adminRoleId = await _context.Roles.Where(r => r.NormalizedName == RoleNames.Admin)
            .Select(r => r.Id).FirstOrDefaultAsync();
        if (adminRoleId == null) return;

        var activeAdminIds = await _context.UserRoles.Where(ur => ur.RoleId == adminRoleId)
            .Join(_context.Users.Where(u => u.Status == UserStatuses.Active), ur => ur.UserId, u => u.Id, (ur, u) => u.Id)
            .ToListAsync();
        if (activeAdminIds.Count == 1 && activeAdminIds[0] == userId)
            throw new ConflictException("Không thể khoá hoặc bỏ role ADMIN của quản trị viên cuối cùng.");
    }

    private async Task<Role> FindRole(string roleId)
        => await _roleManager.FindByIdAsync(roleId) ?? throw new NotFoundException("Không tìm thấy role.");

    private static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private static bool IsSystem(string? name) => name != null && RoleNames.System.Contains(name);

    /// <summary>Loại trùng và từ chối permission ngoài danh mục.</summary>
    private static List<string> ValidatePermissions(IEnumerable<string> permissions)
    {
        var distinct = permissions.Select(p => p?.Trim() ?? "").Distinct().ToList();
        var unknown = distinct.Where(p => !Permissions.All.Contains(p)).ToList();
        if (unknown.Count > 0)
            throw new BadRequestException("Permission không hợp lệ: " + string.Join(", ", unknown));
        return distinct;
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    private static RoleView ToView(Role role, int userCount, IEnumerable<string> permissions)
        => new(role.Id, role.Name!, role.Status, IsSystem(role.Name), userCount, permissions.OrderBy(p => p).ToList());
}
