using cellphones_backend.Data;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace cellPhoneS_backend.Services.Implement;

public class PermissionServiceImpl : IPermissionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public PermissionServiceImpl(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private static string CacheKey(string roleName) => $"perm:role:{roleName.ToUpperInvariant()}";

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(IEnumerable<string> roleNames)
    {
        var result = new HashSet<string>();
        foreach (var roleName in roleNames.Distinct())
        {
            var permissions = await _cache.GetOrCreateAsync(CacheKey(roleName), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                var normalized = roleName.ToUpperInvariant();
                return await _context.RoleClaims
                    .Where(rc => rc.ClaimType == Permissions.ClaimType)
                    .Join(_context.Roles.Where(r => r.NormalizedName == normalized && r.Status == "active"),
                          rc => rc.RoleId, r => r.Id, (rc, r) => rc.ClaimValue!)
                    .ToListAsync();
            });
            result.UnionWith(permissions!);
        }
        return result;
    }

    public void Invalidate(string roleName) => _cache.Remove(CacheKey(roleName));
}
