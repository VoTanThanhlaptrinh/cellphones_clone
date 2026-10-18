using System.Security.Claims;
using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
using Microsoft.AspNetCore.Identity;

namespace cellphones_backend.Data;

/// <summary>
/// Chạy khi khởi động:
/// 1. LUÔN tạo đủ role hệ thống (<see cref="RoleNames.System"/>) và đồng bộ permission mặc định cho ADMIN/MANAGER/STAFF
///    (không phụ thuộc cấu hình). ADMIN luôn đúng bằng <see cref="Permissions.All"/>; MANAGER/STAFF chỉ được nạp khi role
///    chưa có permission nào, để chỉnh sửa qua API không bị ghi đè mỗi lần khởi động.
/// 2. Chỉ khi cấu hình có `AdminSeed:Email` và `AdminSeed:Password`: tạo (hoặc nâng quyền) tài khoản ADMIN.
///    Đặt giá trị bằng `dotnet user-secrets` để mật khẩu không nằm trong repo.
///    - Chưa có user với email đó: tạo mới với mật khẩu đã cấu hình.
///    - Đã có: giữ nguyên mật khẩu hiện tại, chỉ bổ sung các role còn thiếu.
///    - Cấp cả ADMIN và USER vì các route `/api/orders/*`, `/api/carts/*` yêu cầu role USER.
/// </summary>
public static class AdminSeeder
{
    // Role cấp cho tài khoản được seed từ cấu hình.
    private static readonly string[] AccountRoles = { RoleNames.Admin, RoleNames.User };

    private static readonly Dictionary<string, string[]> DefaultPermissions = new()
    {
        [RoleNames.Admin] = Permissions.All.ToArray(),
        [RoleNames.Manager] = Permissions.All.Where(p => p != Permissions.RoleManage).ToArray(),
        [RoleNames.Staff] = new[]
        {
            Permissions.AdminAccess, Permissions.CatalogRead, Permissions.ProductRead, Permissions.InventoryRead,
            Permissions.OrderRead, Permissions.TransactionRead, Permissions.UserRead,
            Permissions.InventoryWrite, Permissions.OrderUpdateStatus
        }
    };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        await SeedRolesAsync(roleManager);
        await SeedPermissionsAsync(roleManager);

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<User>>();

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                Fullname = configuration["AdminSeed:FullName"] ?? "Quản trị viên",
                Birth = new DateOnly(2000, 1, 1),
                Status = "active",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                // Không in mật khẩu; chỉ in lý do (thường là chưa đủ độ mạnh theo cấu hình Identity).
                Console.WriteLine("AdminSeed: không tạo được tài khoản: " + string.Join("; ", created.Errors.Select(e => e.Description)));
                return;
            }
            Console.WriteLine($"AdminSeed: đã tạo tài khoản {email}.");
        }

        foreach (var roleName in AccountRoles)
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
                Console.WriteLine($"AdminSeed: đã cấp role {roleName} cho {email}.");
            }
        }
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        foreach (var roleName in RoleNames.System)
        {
            // Identity so tên role không phân biệt hoa thường, còn JWT/RouteConfig so đúng chữ hoa ("ADMIN").
            // Dữ liệu cũ có thể lưu "Admin": đổi về đúng chữ hoa thay vì tạo role trùng tên chuẩn hoá.
            var existing = await roleManager.FindByNameAsync(roleName);
            if (existing == null)
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = roleName,
                    Status = "active",
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                });
            }
            else if (existing.Name != roleName)
            {
                Console.WriteLine($"AdminSeed: đổi tên role '{existing.Name}' thành '{roleName}'.");
                existing.Name = roleName;
                existing.UpdateDate = DateTime.UtcNow;
                await roleManager.UpdateAsync(existing);
            }
        }
    }

    private static async Task SeedPermissionsAsync(RoleManager<Role> roleManager)
    {
        foreach (var (roleName, permissions) in DefaultPermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;
            var current = (await roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == Permissions.ClaimType).Select(c => c.Value).ToHashSet();

            // ADMIN: luôn đúng bằng danh mục. MANAGER/STAFF: chỉ bổ sung khi role chưa có permission nào
            // (lần seed đầu), để chỉnh sửa qua API không bị ghi đè mỗi lần khởi động.
            if (roleName != RoleNames.Admin && current.Count > 0) continue;

            foreach (var p in permissions.Where(p => !current.Contains(p)))
                await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, p));
            if (roleName == RoleNames.Admin)
                foreach (var p in current.Where(p => !Permissions.All.Contains(p)))
                    await roleManager.RemoveClaimAsync(role, new Claim(Permissions.ClaimType, p));
        }
    }
}
