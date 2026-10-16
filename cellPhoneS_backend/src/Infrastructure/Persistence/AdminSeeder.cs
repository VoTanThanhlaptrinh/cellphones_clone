using cellphones_backend.Models;
using Microsoft.AspNetCore.Identity;

namespace cellphones_backend.Data;

/// <summary>
/// Tạo (hoặc nâng quyền) tài khoản ADMIN khi khởi động, chỉ khi cấu hình có `AdminSeed:Email` và `AdminSeed:Password`.
/// Đặt giá trị bằng `dotnet user-secrets` để mật khẩu không nằm trong repo. Không cấu hình thì không làm gì.
///
/// - Chưa có user với email đó: tạo mới với mật khẩu đã cấu hình.
/// - Đã có: giữ nguyên mật khẩu hiện tại, chỉ bổ sung các role còn thiếu.
/// - Cấp cả ADMIN và USER vì các route `/api/orders/*`, `/api/carts/*` yêu cầu role USER.
/// </summary>
public static class AdminSeeder
{
    private static readonly string[] RoleNames = { "ADMIN", "USER" };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();

        foreach (var roleName in RoleNames)
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

        foreach (var roleName in RoleNames)
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
                Console.WriteLine($"AdminSeed: đã cấp role {roleName} cho {email}.");
            }
        }
    }
}
