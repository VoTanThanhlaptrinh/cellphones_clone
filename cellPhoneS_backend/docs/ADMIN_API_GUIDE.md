# Hướng dẫn xây dựng Backend API cho trang quản trị (Admin)

Tài liệu này là bản thiết kế và hướng dẫn thực hiện toàn bộ backend cho khu quản trị của CellphoneS clone:
phân quyền RBAC theo permission, CRUD cho từng module, API thống kê, và quy ước để phần admin phát triển
song song với phần storefront (user) mà không giẫm chân nhau. Giao diện (`cellphones_clone_ui/src/app/admin/`)
làm sau, dựa trên các hợp đồng API ở đây.

Đọc kèm `cell_phones/CLAUDE.md` (kiến trúc module, namespace, luồng request, auth tập trung).

Mục lục

0. [Hiện trạng và phạm vi](#0-hiện-trạng-và-phạm-vi)
1. [Kiến trúc phát triển song song Admin ↔ User](#1-kiến-trúc-phát-triển-song-song-admin--user)
2. [Phân quyền RBAC theo permission](#2-phân-quyền-rbac-theo-permission)
3. [Quy ước API admin](#3-quy-ước-api-admin)
4. [Danh mục endpoint](#4-danh-mục-endpoint)
5. [API thống kê](#5-api-thống-kê)
6. [Mẫu một tính năng hoàn chỉnh: đổi trạng thái đơn hàng](#6-mẫu-một-tính-năng-hoàn-chỉnh-đổi-trạng-thái-đơn-hàng)
7. [Lộ trình theo phase](#7-lộ-trình-theo-phase)
8. [Checklist khi thêm một API admin](#8-checklist-khi-thêm-một-api-admin)

---

## 0. Hiện trạng và phạm vi

### 0.1 Đã có

| Module | Admin đã có | Ghi chú |
|---|---|---|
| Admin | `AdminQueryController` (`GET /api/admin/products`, `/users`, `/orders`, `/orders/{id}`, `/transactions`) | Chỉ đọc, phân trang; service `AdminQueryServiceImpl` truy vấn thẳng `ApplicationDbContext` |
| Catalog | `POST/PUT/DELETE /api/admin/{categories,brands,series,demands}` | Danh sách vẫn dùng API public (`/api/category`, `/api/brands`…) nên chỉ thấy bản ghi `active` và thiếu `categoryId`/`parentCategoryId` |
| Stores | `POST/PUT/DELETE /api/admin/storehouses` | Chưa có API tồn kho (`Store`: kho × sản phẩm × màu × số lượng) |
| Products | `POST /api/admin/commitments`, `POST /batch`, `PUT/DELETE /{id}`; `POST /api/admin/specifications`, `POST /{id}/details`, `DELETE /{id}` | CRUD sản phẩm lại nằm ở controller **User** (xem 0.2) |
| Media | `POST /api/admin/images`, `POST /upload`, `DELETE /{id}` | |
| Auth | `RouteConfig`: `/api/admin/*` yêu cầu role `ADMIN`; `AdminSeeder` tạo tài khoản ADMIN từ `AdminSeed:Email|Password` | Chỉ có 2 role `ADMIN`, `USER`; không có permission |

### 0.2 Lỗ hổng phải vá trước (Phase 0)

1. **Ai đăng nhập cũng sửa/xoá được sản phẩm.** `POST /api/products`, `PUT/DELETE /api/products/{id}` nằm trong
   `src/Products/Api/Controllers/User/ProductController.cs`. `RouteConfig` chỉ mở `GET /api/products/*`, nên các
   method ghi rơi vào catch-all `/api/*` (chỉ cần đăng nhập). → Chuyển sang `AdminProductController`
   (`/api/admin/products`), xoá khỏi controller User.
2. **`/api/user/*` lộ dữ liệu người khác.** `UserController` (role `USER`):
   `GET /api/user/list/{page}/{pageSize}` liệt kê mọi user; `GET /api/user/{userId}` trả nguyên entity `User`
   (gồm `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`); `DELETE /api/user/{userId}` xoá mềm bất kỳ ai.
   → Chỉ giữ `GET /api/user/me`, `PUT /api/user/me` (trả DTO); list/chi tiết/khoá chuyển sang `/api/admin/users`.
3. **Khoá tài khoản chưa có tác dụng.** Đăng nhập (`AuthServiceImpl.Login`) và refresh (`JwtTokenServiceImpl.RefreshJwtToken`)
   không kiểm tra `User.Status`; `IJwtBlacklistService.IsTokenBlacklistedAsync` không được gọi ở đâu trong pipeline
   nên blacklist JTI vô hiệu. → Login/refresh từ chối user `Status != "active"`; thêm kiểm tra blacklist vào
   `JwtBearerEvents.OnTokenValidated` trong `Program.cs` (xem 4.2).

### 0.3 Còn thiếu (phạm vi của tài liệu)

- Phân quyền chi tiết: nhiều role quản trị (MANAGER, STAFF, role tuỳ chỉnh), permission theo endpoint, API quản lý role.
- Nghiệp vụ ghi: khoá/mở user, gán role, đổi trạng thái đơn, đồng bộ thanh toán thủ công, tồn kho, kiểm duyệt review.
- Danh sách admin cho catalog (gồm bản ghi inactive/deleted).
- API thống kê cho dashboard.
- Audit log các thao tác ghi của admin.
- Hạ tầng còn hổng mà admin cần: validator FluentValidation không được chạy (mục 6 bước 2); huỷ đơn không hoàn tồn kho (4.6).

---

## 1. Kiến trúc phát triển song song Admin ↔ User

### 1.1 Nguyên tắc: tách bề mặt API, dùng chung domain

```
                 ┌──────────── storefront (Angular /store) ────────────┐   ┌──── admin (Angular /admin) ────┐
                 │                                                     │   │                                │
HTTP             │  /api/products, /api/orders, /api/carts, ...        │   │  /api/admin/**                 │
                 └──────────────────────────┬──────────────────────────┘   └───────────────┬────────────────┘
                                            │                                              │
CentralizedAuthMiddleware (RouteConfig)     │  anonymous / USER                            │  admin.access (thô)
RequirePermission (attribute trên action)   │  —                                           │  product.write, order.update-status… (chi tiết)
                                            ▼                                              ▼
Api        src/<Module>/Api/Controllers/User/*Controller          src/<Module>/Api/Controllers/Admin/Admin*Controller
                                            │                                              │
Application         <Name>Service (nghiệp vụ ghi dùng chung) ◄─────────────────────────────┤
                    <Name>Service (đọc cho storefront)          Admin<Module>QueryService (đọc cho admin, CQRS-lite)
                                            │                                              │
Domain                       Entity + quy tắc chuyển trạng thái (Order.MarkPaid/Cancel/Ship/Complete…)  ← một luật cho cả hai phía
                                            │                                              │
Infrastructure                         Repository / ApplicationDbContext (chung)
```

- **Route:** mọi endpoint admin có prefix `/api/admin/`, nằm trong `src/<Module>/Api/Controllers/Admin/`, class tên
  `Admin<Name>Controller`, namespace `cellphones_backend.Controllers.Admin` (như các controller admin hiện có).
  Không đặt endpoint ghi dành cho admin trong controller `User/` (đây chính là nguyên nhân lỗ hổng 0.2.1).
- **Ghi:** dùng lại service nghiệp vụ của module (`ProductService.CreateProduct`, `BrandService.UpdateBrand`…) khi
  luật giống nhau; thêm method mới vào service đó chứ không copy luật sang admin.
- **Đọc:** admin cần lọc/sắp xếp/xem bản ghi đã xoá, khác hẳn storefront. Đặt vào service đọc riêng
  `Admin<Module>QueryService` (`src/<Module>/Application/Services/Admin/`): `AsNoTracking`, projection thẳng sang DTO,
  không gọi lại service của storefront và ngược lại. Nhờ vậy sửa trang chủ không làm vỡ bảng admin.
- **Quy tắc trạng thái** nằm trên entity (như `Order.MarkPaid/Cancel` hiện tại). Admin đổi trạng thái đơn cũng gọi
  method của entity, nên user và admin không thể "lệch luật".
- **DTO** của admin riêng, đặt trong `src/<Module>/Application/DTOs/Admin/`: `Admin<Name>View`, `Admin<Name>Detail`,
  `Admin<Name>Filter`, `Create<Name>Request`, `Update<Name>Request`. Không bao giờ trả entity ra API.
- **Module `Admin`** chỉ giữ phần xuyên module: thống kê, role/permission, audit log, `GET /api/admin/me`.
  `AdminQueryController` hiện tại có thể giữ nguyên; khi một module cần thêm API admin, dời phần đọc của module đó
  về `Admin<Module>QueryService` của module (không cần làm một lần).

### 1.2 Cấu trúc thư mục mẫu (module Orders)

```
src/Orders/
  Api/Controllers/
    User/OrderController.cs                 # /api/orders/*         (dời OrderController/CheckoutController vào User/ khi tiện)
    Admin/AdminOrderController.cs           # /api/admin/orders/*
  Application/
    DTOs/                                   # DTO storefront
    DTOs/Admin/AdminOrderFilter.cs
    DTOs/Admin/UpdateOrderStatusRequest.cs
    Services/OrderService.cs + OrderServiceImpl.cs          # nghiệp vụ chung
    Services/Admin/AdminOrderService.cs + AdminOrderServiceImpl.cs
    Validation/Admin/UpdateOrderStatusRequestValidator.cs
  Domain/Entities/Order.cs                  # thêm Ship()/Complete()/CanTransitionTo()
```

### 1.3 Hợp đồng API trước, code sau

- Bật OpenAPI (gói `Microsoft.AspNetCore.OpenApi` đã có trong `cellPhoneS_backend.csproj`) **chỉ ở Development**:

  ```csharp
  // Program.cs
  builder.Services.AddOpenApi();
  ...
  if (app.Environment.IsDevelopment())
  {
      app.MapOpenApi(); // GET /openapi/v1.json
  }
  ```
  `/openapi/*` phải được thêm vào `RouteConfig` dạng `AllowAnonymous` (chỉ map ở Development nên production trả 404).
- Mỗi resource admin có file `http/admin/<resource>.http` (theo mẫu `http/brand.http`) để test tay và làm tài liệu sống.
- Khi bắt đầu một tính năng admin: viết DTO + chữ ký controller trước (trả `NotImplementedException` → 501), merge
  sớm để frontend dựng giao diện theo `/openapi/v1.json`, rồi mới làm service.

### 1.4 Quy tắc tránh xung đột khi nhiều nhánh làm song song

- Một tính năng admin = các **file mới** trong module tương ứng. Không sửa controller/service của storefront trừ khi
  cần thêm method nghiệp vụ dùng chung (khi đó báo người làm phía user review).
- Chỉ có 5 điểm chạm dùng chung; thêm dòng **trong khối có comment theo module** để merge dễ:
  - `src/Infrastructure/Auth/RouteConfig.cs` (hầu như không phải sửa: admin dùng `[RequirePermission]`)
  - `src/Shared/Constants/Permissions.cs`
  - `src/Application/DependencyInjection/ServiceCollectionExtensions.cs`
  - `src/Infrastructure/DependencyInjection/RepositoryServiceCollectionExtensions.cs`
  - `src/Infrastructure/Persistence/ApplicationDbContext.cs` (khi có entity mới)
- Migration đặt tên `Admin_<Feature>` (vd. `Admin_AuditLog`). Hai nhánh cùng sinh migration thì nhánh merge sau phải
  xoá migration của mình và `dotnet ef migrations add` lại trên nền đã merge (không merge tay `ModelSnapshot`).
- Thay đổi entity dùng chung (thêm cột, thêm method trạng thái) làm thành commit riêng, nhỏ, merge trước.

---

## 2. Phân quyền RBAC theo permission

### 2.1 Mô hình

```
User ──(AspNetUserRoles)──► Role ──(AspNetRoleClaims, ClaimType = "permission")──► permission string
```

- **Không tạo bảng mới.** Identity đã có `AspNetRoleClaims`; mỗi permission của role là một dòng claim
  `("permission", "product.write")`. Dùng `RoleManager<Role>.GetClaimsAsync/AddClaimAsync/RemoveClaimAsync`.
- Bảng `GrantRole` (module Account) đang không được dùng cho phân quyền; giữ nguyên, không dựa vào nó.
- **Danh mục permission là hằng số trong code** (`src/Shared/Constants/Permissions.cs`). DB chỉ lưu permission nào
  gán cho role nào. API không cho tạo permission mới, chỉ gán permission có trong danh mục.
- **Không đưa permission vào JWT.** JWT vẫn chỉ có role (`JwtTokenServiceImpl.GenerateJwtToken`). Permission được tra từ
  role lúc request (có cache), nên sửa quyền của một role có hiệu lực ngay, token không phình.
  Đổi role của một user có hiệu lực ở lần refresh kế tiếp (access token sống 15 phút); muốn có hiệu lực ngay thì
  thu hồi phiên của user (xem 4.2, `PATCH /api/admin/users/{id}/status`).

### 2.2 Danh mục permission

```csharp
// src/Shared/Constants/Permissions.cs
namespace cellPhoneS_backend.Constants;

/// <summary>
/// Danh mục permission của khu quản trị, dạng "<resource>.<action>". Role chỉ được gán các giá trị có trong <see cref="All"/>.
/// Thêm permission mới: thêm hằng số, thêm vào All, cập nhật bảng ở docs/ADMIN_API_GUIDE.md.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    /// <summary>Được vào khu /api/admin/* (mọi role quản trị phải có).</summary>
    public const string AdminAccess = "admin.access";

    // Catalog (danh mục, thương hiệu, dòng, nhu cầu)
    public const string CatalogRead = "catalog.read";
    public const string CatalogWrite = "catalog.write";

    // Products (sản phẩm, thông số, cam kết)
    public const string ProductRead = "product.read";
    public const string ProductWrite = "product.write";
    public const string ProductDelete = "product.delete";

    // Media
    public const string MediaWrite = "media.write";

    // Stores (kho, tồn kho)
    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";

    // Orders & Payments
    public const string OrderRead = "order.read";
    public const string OrderUpdateStatus = "order.update-status";
    public const string OrderCancel = "order.cancel";
    public const string TransactionRead = "transaction.read";

    // Account
    public const string UserRead = "user.read";
    public const string UserLock = "user.lock";
    public const string UserAssignRole = "user.assign-role";
    public const string RoleManage = "role.manage";

    // Reviews
    public const string ReviewModerate = "review.moderate";

    // Admin (xuyên module)
    public const string StatsView = "stats.view";
    public const string AuditRead = "audit.read";

    public static readonly IReadOnlyList<string> All = new[]
    {
        AdminAccess,
        CatalogRead, CatalogWrite,
        ProductRead, ProductWrite, ProductDelete,
        MediaWrite,
        InventoryRead, InventoryWrite,
        OrderRead, OrderUpdateStatus, OrderCancel, TransactionRead,
        UserRead, UserLock, UserAssignRole, RoleManage,
        ReviewModerate,
        StatsView, AuditRead
    };
}
```

```csharp
// src/Shared/Constants/RoleNames.cs
namespace cellPhoneS_backend.Constants;

public static class RoleNames
{
    public const string Admin = "ADMIN";     // toàn quyền, không sửa/xoá được
    public const string Manager = "MANAGER";
    public const string Staff = "STAFF";
    public const string User = "USER";       // khách hàng, không có permission admin

    /// <summary>Role hệ thống: không xoá, không đổi tên qua API.</summary>
    public static readonly IReadOnlyList<string> System = new[] { Admin, Manager, Staff, User };
}
```

### 2.3 Role mặc định (seed)

| Permission | ADMIN | MANAGER | STAFF |
|---|:-:|:-:|:-:|
| `admin.access` | ✓ | ✓ | ✓ |
| `catalog.read`, `product.read`, `inventory.read`, `order.read`, `transaction.read`, `user.read` | ✓ | ✓ | ✓ |
| `catalog.write`, `product.write`, `media.write` | ✓ | ✓ | |
| `product.delete` | ✓ | ✓ | |
| `inventory.write`, `order.update-status` | ✓ | ✓ | ✓ |
| `order.cancel`, `review.moderate` | ✓ | ✓ | |
| `user.lock`, `user.assign-role` | ✓ | ✓ | |
| `stats.view` | ✓ | ✓ | |
| `audit.read` | ✓ | ✓ | |
| `role.manage` | ✓ | | |

Luật bảo vệ:
- Permission của `ADMIN` luôn là `Permissions.All` (seed lại mỗi lần khởi động; API không sửa được).
- Không được xoá role hệ thống; không được bỏ role `ADMIN` khỏi chính mình; không được bỏ/khoá ADMIN cuối cùng.
- Chỉ người có role `ADMIN` mới gán được role `ADMIN` cho người khác (`user.assign-role` không đủ).
- MANAGER không có `role.manage` nên không tự nâng quyền được.

Mở rộng `AdminSeeder` (`src/Infrastructure/Persistence/AdminSeeder.cs`): tạo đủ 4 role trong `RoleNames.System`
(giữ logic chuẩn hoá chữ hoa hiện có), rồi đồng bộ claim cho 3 role quản trị:

```csharp
// Trong AdminSeeder, sau vòng tạo role
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
```

`AdminSeeder` hiện chỉ chạy khi có `AdminSeed:Email|Password`. Tách phần seed role + permission ra chạy **luôn**
(không phụ thuộc cấu hình), phần tạo tài khoản admin giữ điều kiện cũ.

### 2.4 Tra permission: `IPermissionService`

```csharp
// src/Admin/Application/Services/IPermissionService.cs
namespace cellPhoneS_backend.Services.Interface;

public interface IPermissionService
{
    /// <summary>Hợp các permission của những role đã cho (tên role lấy từ JWT).</summary>
    Task<IReadOnlySet<string>> GetPermissionsAsync(IEnumerable<string> roleNames);
    /// <summary>Gọi sau khi sửa permission/xoá role để request kế tiếp đọc lại từ DB.</summary>
    void Invalidate(string roleName);
}
```

```csharp
// src/Admin/Application/Services/PermissionServiceImpl.cs
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
```

Backend hiện chạy một instance (Render) nên `IMemoryCache` là đủ. Khi scale nhiều instance, đổi sang Redis
(`IConnectionMultiplexer` đã đăng ký) hoặc giảm TTL xuống 1 phút.

### 2.5 Thực thi: hai lớp

**Lớp 1 – thô, tập trung (`RouteConfig` + `CentralizedAuthMiddleware`):** ai không có `admin.access` bị chặn ở cửa
`/api/admin/*`. Thêm `RequiredPermissions` vào `RoutePolicy`:

```csharp
// src/Infrastructure/Auth/RoutePolicy.cs (thêm thuộc tính)
public List<string> RequiredPermissions { get; set; } = new List<string>(); // phải có tất cả
```

```csharp
// src/Infrastructure/Auth/RouteConfig.cs — thay dòng /api/admin/* hiện tại
new RoutePolicy { UrlPattern = "/api/admin/*", RequiredPermissions = new List<string> { Permissions.AdminAccess } },
```

```csharp
// src/Infrastructure/Auth/CentralizedAuthMiddleware.cs — sau đoạn kiểm tra RequiredRoles
if (policy.RequiredPermissions.Count > 0)
{
    // Middleware là singleton: lấy service scoped từ RequestServices.
    var permissionService = context.RequestServices.GetRequiredService<IPermissionService>();
    var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    var granted = await permissionService.GetPermissionsAsync(roles);
    if (!policy.RequiredPermissions.All(granted.Contains))
    {
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        await context.Response.WriteAsJsonAsync(new ApiResponse<object?>("Forbidden: Insufficient permissions.", null));
        return;
    }
}
```

Nhân tiện đổi các body 401/403 của middleware sang `ApiResponse<object?>` để frontend chỉ phải đọc một định dạng lỗi.

**Lớp 2 – chi tiết, cạnh action (`[RequirePermission]`):** mỗi action admin khai báo permission cần có. Đây là ngoại lệ
có chủ đích với nguyên tắc "auth tập trung" trong CLAUDE.md: nếu dồn hết vào `RouteConfig` thì mọi nhánh admin đều
sửa cùng một file và quy tắc khớp prefix không phân biệt được `PUT /orders/{id}/status` với `POST /orders/{id}/sync-payment`.
`RouteConfig` vẫn là lớp chặn ngoài cùng (anonymous/USER/admin.access).

```csharp
// src/Api/Filters/RequirePermissionAttribute.cs
using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace cellPhoneS_backend.Filters;

/// <summary>
/// Yêu cầu user hiện tại có các permission đã cho (tra qua role trong JWT). Đặt trên action hoặc controller;
/// nhiều attribute thì phải thoả tất cả.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public IReadOnlyList<string> Permissions { get; }

    public RequirePermissionAttribute(params string[] permissions) => Permissions = permissions;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new ApiResponse<object?>("Unauthorized: Authentication required.", null));
            return;
        }

        var service = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var granted = await service.GetPermissionsAsync(user.FindAll(ClaimTypes.Role).Select(c => c.Value));
        if (!Permissions.All(granted.Contains))
        {
            context.Result = new ObjectResult(new ApiResponse<object?>("Forbidden: Insufficient permissions.", null))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
```

Dùng:

```csharp
[Route("api/admin/brands")]
[ApiController]
[RequirePermission(Permissions.CatalogRead)]          // mặc định cho cả controller
public class AdminBrandController : BaseController
{
    [HttpPost]
    [RequirePermission(Permissions.CatalogWrite)]     // thêm yêu cầu cho action ghi
    public async Task<ActionResult<ApiResponse<long>>> CreateBrand(...) { ... }
}
```

**Fail-closed:** quên gắn attribute thì endpoint chỉ còn được bảo vệ bởi `admin.access` (STAFF gọi được API ghi).
Thêm kiểm tra lúc khởi động trong Development để phát hiện sớm:

```csharp
// Program.cs, sau app.MapControllers(), chỉ trong Development
if (app.Environment.IsDevelopment())
{
    var actions = app.Services.GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>()
        .ActionDescriptors.Items;
    var missing = actions
        .Where(a => a.AttributeRouteInfo?.Template?.StartsWith("api/admin", StringComparison.OrdinalIgnoreCase) == true)
        .Where(a => !a.EndpointMetadata.OfType<cellPhoneS_backend.Filters.RequirePermissionAttribute>().Any())
        .Select(a => a.DisplayName);
    foreach (var name in missing)
        app.Logger.LogWarning("Admin endpoint thiếu [RequirePermission]: {Action}", name);
}
```

### 2.6 `GET /api/admin/me`

Trả thông tin để UI ẩn/hiện menu và nút (backend vẫn là nơi quyết định):

```json
{
  "message": "Success",
  "data": {
    "id": "8f1c…",
    "email": "admin@example.com",
    "fullName": "Quản trị viên",
    "roles": ["ADMIN", "USER"],
    "permissions": ["admin.access", "catalog.read", "catalog.write", "..."]
  }
}
```

Frontend (`core/auth/admin.guard`) sau này đổi từ "JWT có role ADMIN" sang "gọi `/api/admin/me` thành công".

---

## 3. Quy ước API admin

### 3.1 Response và lỗi

- Thành công: `Ok(new ApiResponse<T>(message, data))` (`cellphones_backend.DTOs.Responses`). Ghi không có dữ liệu
  trả `ApiResponse<string>(message, "")` như các controller admin hiện tại; tạo mới trả id.
- Lỗi nghiệp vụ: service ném `BadRequestException` (400), `NotFoundException` (404), `ConflictException` (409),
  `ForbiddenException` (403) (`src/Application/Common/Exceptions.cs`, namespace `cellPhoneS_backend.Services`);
  `ApiExceptionFilter` đổi sang status + `ApiResponse`. Controller không `try/catch`.
- Lỗi validation (FluentValidation, ModelState) → 400.

### 3.2 Danh sách (list)

```
GET /api/admin/<resource>?pageIndex=1&pageSize=20&search=&status=&sortBy=createDate&sortDir=desc&from=&to=
→ ApiResponse<PagedResult<Admin<Name>View>>
```

- `pageIndex` từ 1; `pageSize` kẹp trong [1, 100].
- `status`: bỏ trống = mọi trạng thái **trừ** `deleted`; `status=deleted` để xem thùng rác.
- `sortBy` chỉ nhận giá trị trong whitelist của từng resource (không đưa thẳng vào `OrderBy` động).
- `from/to`: ngày theo giờ Việt Nam (`yyyy-MM-dd`), backend đổi sang khoảng UTC `[from 00:00+07, to+1 00:00+07)`.

Gom logic phân trang đang nằm riêng trong `AdminQueryServiceImpl.Normalize` thành helper chung:

```csharp
// src/Application/DTOs/Common/PagingExtensions.cs
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.DTOs.Responses;

public static class PagingExtensions
{
    public const int MaxPageSize = 100;

    public static (int pageIndex, int pageSize) NormalizePaging(int pageIndex, int pageSize)
        => (Math.Max(pageIndex, 1), Math.Clamp(pageSize, 1, MaxPageSize));

    /// <summary>Đếm + lấy một trang. <paramref name="query"/> phải đã được sắp xếp.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageIndex, int pageSize)
    {
        (pageIndex, pageSize) = NormalizePaging(pageIndex, pageSize);
        var total = await query.CountAsync();
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<T>(items, total, pageIndex, pageSize);
    }
}
```

Filter nhận bằng một class `[FromQuery]` để thêm tham số không đổi chữ ký action:

```csharp
// src/Application/DTOs/Common/AdminListQuery.cs
namespace cellPhoneS_backend.DTOs.Responses;

public class AdminListQuery
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; }
    public string SortDir { get; set; } = "desc";
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
// Filter riêng của từng resource kế thừa: public class AdminOrderFilter : AdminListQuery { public string? Type { get; set; } }
```

### 3.3 Ghi

| Thao tác | Method + route | Ghi chú |
|---|---|---|
| Tạo | `POST /api/admin/<res>` | Trả id |
| Sửa toàn phần | `PUT /api/admin/<res>/{id}` | id URL ≠ id body → 400 (như `AdminBrandController`) |
| Đổi trạng thái | `PATCH /api/admin/<res>/{id}/status` | Body `{ "status": "...", "reason": "..." }` |
| Xoá | `DELETE /api/admin/<res>/{id}` | Xoá mềm: `Status = "deleted"` |
| Khôi phục | `PATCH /api/admin/<res>/{id}/restore` | `deleted` → `active` |

- Id người thao tác: thêm helper vào `BaseController` thay cho đoạn `User.FindFirstValue(...)` lặp ở mọi action:

  ```csharp
  // src/Api/Controllers/BaseController.cs
  protected string CurrentUserId =>
      User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? throw new UnauthorizedException("User not authenticated");
  ```
- Mọi entity có `UpdateBy/UpdateDate` thì service ghi đủ hai trường.
- Thao tác ảnh hưởng cache phải làm mới cache: sửa sản phẩm → `ProductSearchService.InitializeAsync()` (hoặc thêm
  method cập nhật một sản phẩm), sửa danh mục → xoá cache trang chủ (`CacheService`).

### 3.4 Audit log

Bảng mới (module Admin), ghi tự động cho mọi request ghi dưới `/api/admin/*`, service không phải gọi tay:

```csharp
// src/Admin/Domain/Entities/AuditLog.cs
namespace cellphones_backend.Models;

public class AuditLog
{
    public long Id { get; set; }
    [Required, MaxLength(450)] public string UserId { get; set; } = default!;
    [MaxLength(256)] public string? UserEmail { get; set; }
    [Required, MaxLength(10)] public string Method { get; set; } = default!;      // POST/PUT/PATCH/DELETE
    [Required, MaxLength(256)] public string Path { get; set; } = default!;       // /api/admin/orders/15/status
    [MaxLength(64)] public string? Resource { get; set; }                         // orders
    [MaxLength(64)] public string? ResourceId { get; set; }                       // 15
    public int StatusCode { get; set; }
    [MaxLength(4000)] public string? Payload { get; set; }                        // body đã cắt, bỏ trường nhạy cảm
    [MaxLength(64)] public string? Ip { get; set; }
    public DateTime CreateDate { get; set; }
}
```

- Ghi bằng `IAsyncActionFilter` đăng ký global (`AddControllers(options => options.Filters.Add<AdminAuditFilter>())`),
  chỉ xử lý khi path bắt đầu bằng `/api/admin/` và method khác GET; ghi **sau** khi action chạy để có `StatusCode`.
- Payload: serialize `context.ActionArguments` (bỏ `IFormFile`, che các key chứa `password`), cắt 4000 ký tự.
- Lỗi khi ghi audit chỉ log, không làm hỏng request.
- Migration `Admin_AuditLog`, index `(CreateDate)`, `(UserId, CreateDate)`.

---

## 4. Danh mục endpoint

Ký hiệu trạng thái: **Có** = đã có, chỉ gắn permission · **Sửa** = có nhưng phải đổi · **Mới** = chưa có.
Mọi route dưới đây có prefix `/api/admin`, response bọc trong `ApiResponse<T>`.

### 4.1 Phân quyền (module Admin)

| Method | Route | Permission | Request | Response | TT |
|---|---|---|---|---|---|
| GET | `/me` | `admin.access` | | `AdminMeView` (2.6) | Mới |
| GET | `/permissions` | `role.manage` | | `List<PermissionView>` (`code`, `group`, `description`) | Mới |
| GET | `/roles` | `role.manage` | | `List<RoleView>` (`id`, `name`, `status`, `isSystem`, `userCount`, `permissions`) | Mới |
| GET | `/roles/{id}` | `role.manage` | | `RoleView` | Mới |
| POST | `/roles` | `role.manage` | `{ name, permissions[] }` | id | Mới |
| PUT | `/roles/{id}` | `role.manage` | `{ id, name }` (role hệ thống không đổi tên) | | Mới |
| PUT | `/roles/{id}/permissions` | `role.manage` | `{ permissions[] }` (thay toàn bộ; ADMIN → 403) | | Mới |
| DELETE | `/roles/{id}` | `role.manage` | (role hệ thống hoặc còn user → 409) | | Mới |
| GET | `/audit-logs` | `audit.read` | `AdminListQuery` + `userId`, `resource` | `PagedResult<AuditLogView>` | Mới |

Sau mọi thao tác ghi role: `IPermissionService.Invalidate(roleName)`. Permission gửi lên không có trong
`Permissions.All` → 400. Tên role chuẩn hoá chữ hoa (JWT/`RouteConfig` so đúng chữ hoa, xem `AdminSeeder`).

### 4.2 Người dùng (module Account)

| Method | Route | Permission | Request | Response | TT |
|---|---|---|---|---|---|
| GET | `/users` | `user.read` | `AdminListQuery` + `role` | `PagedResult<AdminUserView>` | Sửa (thêm lọc status/role, sort) |
| GET | `/users/{id}` | `user.read` | | `AdminUserDetail`: thông tin, roles, `orderCount`, `totalSpent`, 5 đơn gần nhất, ngày đăng nhập gần nhất (nếu lưu) | Mới |
| PATCH | `/users/{id}/status` | `user.lock` | `{ status: "active" \| "locked", reason }` | | Mới |
| PUT | `/users/{id}/roles` | `user.assign-role` | `{ roles[] }` | | Mới |

Phía user (sửa `UserController`, `/api/user/*`): bỏ `list/{page}/{pageSize}`, `GET/DELETE {userId}`; thêm
`GET /api/user/me`, `PUT /api/user/me` (tên, ngày sinh, số điện thoại) trả `UserResponse`.

Khoá tài khoản phải có hiệu lực ngay, gồm 3 việc:
1. `AuthServiceImpl.Login` và `JwtTokenServiceImpl.RefreshJwtToken` từ chối khi `user.Status != "active"`
   (`UnauthorizedException`, message "Tài khoản đã bị khoá").
2. Khi khoá: ghi Redis `user-revoked:{userId}` = thời điểm khoá, TTL = thời gian sống access token (15 phút).
3. Trong `Program.cs`, `AddJwtBearer(... Events = new JwtBearerEvents { OnTokenValidated = ... })`: từ chối token nếu
   JTI nằm trong blacklist (`IJwtBlacklistService.IsTokenBlacklistedAsync`) hoặc có `user-revoked:{sub}` mới hơn `iat`.
   Refresh token lưu dạng `rt:{guid}` không tra được theo user, nên bước 1 là thứ chặn refresh.

Luật: không tự khoá mình, không khoá/bỏ role ADMIN cuối cùng; chỉ ADMIN gán được role ADMIN; role gán phải tồn tại
và `active`; luôn giữ role `USER` cho tài khoản khách (các route `/api/orders/*`, `/api/carts/*` cần nó).

### 4.3 Sản phẩm (module Products)

| Method | Route | Permission | Request | Response | TT |
|---|---|---|---|---|---|
| GET | `/products` | `product.read` | `AdminListQuery` + `categoryId`, `brandId`, `lowStock` | `PagedResult<AdminProductView>` | Sửa (thêm lọc) |
| GET | `/products/{id}` | `product.read` | | `AdminProductDetail` (đủ dữ liệu cho form sửa: ảnh, màu, thông số, cam kết, danh mục, tồn kho theo kho) | Mới |
| POST | `/products` | `product.write` | `AddProductRequest` (đã có) | id | Sửa (dời từ `POST /api/products`) |
| PUT | `/products/{id}` | `product.write` | `UpdateProductRequest` (đã có) | | Sửa (dời từ `PUT /api/products/{id}`) |
| PATCH | `/products/{id}/status` | `product.write` | `{ status: "active" \| "inactive" }` | | Mới |
| DELETE | `/products/{id}` | `product.delete` | | | Sửa (dời từ `DELETE /api/products/{id}`) |
| PATCH | `/products/{id}/restore` | `product.delete` | | | Mới |
| POST/PUT/DELETE | `/commitments`, `/commitments/batch`, `/commitments/{id}` | `product.write` | (đã có) | | Có |
| POST/DELETE | `/specifications`, `/specifications/{id}/details`, `/specifications/{id}` | `product.write` | (đã có) | | Có |

Dời `CreateProduct/UpdateProduct/DeleteProduct` khỏi `src/Products/Api/Controllers/User/ProductController.cs` sang
`src/Products/Api/Controllers/Admin/AdminProductController.cs`; service `ProductService` giữ nguyên. Sau khi ghi,
làm mới `ProductSearchService` (tìm kiếm in-memory) để storefront thấy thay đổi.

### 4.4 Danh mục (module Catalog)

| Method | Route | Permission | TT |
|---|---|---|---|
| GET | `/categories`, `/brands`, `/series`, `/demands` | `catalog.read` | Mới — danh sách admin: mọi trạng thái, có `parentCategoryId`/`categoryId`/`brandId`, `productCount`, lọc + phân trang |
| GET | `/categories/{id}` … `/demands/{id}` | `catalog.read` | Mới |
| POST/PUT/DELETE | `/categories`, `/brands`, `/series`, `/demands` (+`/{id}`) | `catalog.write` | Có |
| PATCH | `/<res>/{id}/restore` | `catalog.write` | Mới |

Sau khi có list admin, frontend đổi `listUrl` trong `catalog-resources.ts` từ API public sang `admin/<res>` và bỏ
`createOnly` cho các trường quan hệ. Xoá danh mục còn sản phẩm/con đang `active` → 409.

### 4.5 Kho và tồn kho (module Stores)

| Method | Route | Permission | Request | Response | TT |
|---|---|---|---|---|---|
| GET | `/storehouses` | `inventory.read` | `AdminListQuery` + `province` | `PagedResult<AdminStoreHouseView>` (kèm tổng tồn) | Mới |
| POST/PUT/DELETE | `/storehouses`, `/storehouses/{id}` | `inventory.write` | (đã có) | | Có |
| GET | `/inventory` | `inventory.read` | `productId`, `storeHouseId`, `colorId`, `lowStock` (ngưỡng), phân trang | `PagedResult<InventoryRowView>` (`storeHouse`, `product`, `color`, `quantity`, `updateDate`) | Mới |
| PUT | `/inventory` | `inventory.write` | `{ storeHouseId, productId, colorId, quantity }` (đặt tuyệt đối, tạo dòng nếu chưa có) | | Mới |
| POST | `/inventory/adjust` | `inventory.write` | `{ storeHouseId, productId, colorId, delta, reason }` (kết quả < 0 → 400) | số lượng mới | Mới |

Khoá chính `Store` là (`StoreHouseId`, `ProductId`, `ColorId`). Điều chỉnh tồn có thể đụng độ với đặt hàng
(`OrderServiceImpl` trừ kho trong transaction): dùng `UPDATE ... SET "Quantity" = "Quantity" + @delta WHERE ... AND "Quantity" + @delta >= 0`
(`ExecuteUpdateAsync`) thay vì đọc–sửa–ghi. `reason` được lưu qua audit log.

### 4.6 Đơn hàng và giao dịch (modules Orders, Payments)

| Method | Route | Permission | Request | Response | TT |
|---|---|---|---|---|---|
| GET | `/orders` | `order.read` | `AdminOrderFilter`: `status`, `type`, `from`, `to`, `search` (mã đơn/email/tên) | `PagedResult<AdminOrderView>` | Sửa (thêm lọc) |
| GET | `/orders/{id}` | `order.read` | | `AdminOrderView` + địa chỉ/kho nhận, `paymentAttempt`, giao dịch | Sửa |
| PATCH | `/orders/{id}/status` | `order.update-status` | `{ status: "Shipping" \| "Completed", note }` | `AdminOrderView` | Mới (mẫu ở mục 6) |
| POST | `/orders/{id}/cancel` | `order.cancel` | `{ reason }` | | Mới |
| POST | `/orders/{id}/sync-payment` | `order.update-status` | | trạng thái mới | Mới |
| GET | `/transactions` | `transaction.read` | `provider`, `status`, `from`, `to`, `orderId` | `PagedResult<AdminTransactionView>` | Sửa (thêm lọc) |

Luật trạng thái (đặt trên `Order`, cạnh `MarkPaid/Cancel`; hằng số trong `OrderStatuses`):

```
Pending ──MarkPaid()──► Paid ──Ship()──► Shipping ──Complete()──► Completed
   │                     │                  │
   └──────── Cancel() ───┴──────────────────┘          (Completed/Cancelled: kết thúc)
```

- Admin **không** được chuyển tay `Pending → Paid` cho đơn online (gateway là nguồn sự thật); dùng `sync-payment`,
  gọi lại đúng luồng `OrderService.HandlePaymentCallback(orderId)` (hỏi gateway `GET /status` rồi `MarkOrderPaid`).
- Đơn `pickup` thanh toán tại quầy: cho phép `Pending → Completed` qua `Complete()` (ghi rõ luật này trên entity).
- Huỷ đơn đã `Paid` online: chỉ đổi trạng thái, việc hoàn tiền ngoài phạm vi (gateway chưa có refund); hiển thị cảnh
  báo trong response message.
- Tồn kho bị trừ ngay lúc tạo đơn (`OrderServiceImpl`, `store.Quantity -= takeQty`, mỗi `OrderDetail` ghi
  `StoreHouseId` + `ColorId`), nhưng huỷ đơn hiện **không** hoàn lại (`DeleteOrder` chỉ gọi `order.Cancel`). Thêm một
  hàm hoàn kho dùng chung (cộng lại theo từng `OrderDetail`, cùng transaction với `Cancel`) cho cả huỷ phía khách và
  phía admin.

### 4.7 Đánh giá (module Products)

| Method | Route | Permission | Request | Response | TT |
|---|---|---|---|---|---|
| GET | `/reviews` | `review.moderate` | `productId`, `rating`, `status`, `search`, phân trang | `PagedResult<AdminReviewView>` (sản phẩm, người viết, sao, nội dung, ngày) | Mới |
| PATCH | `/reviews/{id}/status` | `review.moderate` | `{ status: "visible" \| "hidden" }` | | Mới |
| DELETE | `/reviews/{id}` | `review.moderate` | | | Mới |

`Review` chưa có cột trạng thái: thêm `Status` (mặc định `"visible"`) bằng migration `Admin_ReviewStatus`; API public
`GET /api/reviews/product/*` và điểm trung bình chỉ tính review `visible`.

### 4.8 Media

`POST /images`, `POST /images/upload`, `DELETE /images/{id}` — permission `media.write`. **Có**.

---

## 5. API thống kê

Controller `AdminStatisticsController` (`src/Admin/Api/Controllers/`), route `api/admin/stats`, cả controller
`[RequirePermission(Permissions.StatsView)]`; service `IAdminStatisticsService` truy vấn `ApplicationDbContext` chỉ đọc.

### 5.1 Định nghĩa thống nhất

- **Đơn tính doanh thu:** `Status ∈ { Paid, Shipping, Completed }` (khai báo một lần:
  `OrderStatuses.Revenue = new[] { Paid, Shipping, Completed }`).
- **Doanh thu một đơn:** `Σ OrderDetail.Price × Quantity + Σ FeeDetail.Value` — giống `Order.GetTotalAmount()`.
  Doanh thu hàng hoá (không phí ship) dùng cho thống kê theo sản phẩm/danh mục.
- **Múi giờ:** `CreateDate` lưu UTC (`timestamp with time zone`). Mọi nhóm theo ngày/tuần/tháng dùng giờ Việt Nam
  (`Asia/Ho_Chi_Minh`), `from/to` hiểu theo giờ Việt Nam.
- **Tham số chung:** `from`, `to` (`yyyy-MM-dd`, mặc định 30 ngày gần nhất, tối đa 366 ngày), `groupBy=day|week|month`.
- **Kỳ so sánh:** cùng độ dài, ngay trước `from`. Phần trăm thay đổi = `(cur - prev) / prev × 100`, `prev = 0` → `null`.
- **Cache:** `IMemoryCache` 5 phút, key gồm tên endpoint + tham số. Dashboard không cần realtime.

### 5.2 Endpoint

| Route | Tham số | Response (`data`) |
|---|---|---|
| `GET /stats/overview` | `from`, `to` | `{ revenue, orders, paidOrders, averageOrderValue, newCustomers, cancelledRate, revenueChangePct, ordersChangePct, newCustomersChangePct }` |
| `GET /stats/revenue` | `from`, `to`, `groupBy` | `[{ period: "2026-10-01", revenue, orders }]` — đủ mọi kỳ, kỳ trống = 0 |
| `GET /stats/orders-by-status` | `from`, `to` | `[{ status, count }]` |
| `GET /stats/top-products` | `from`, `to`, `limit` (≤50, mặc định 10), `orderBy=quantity\|revenue` | `[{ productId, name, imageUrl, quantity, revenue }]` |
| `GET /stats/revenue-by-category` | `from`, `to` | `[{ categoryId, name, revenue, quantity }]` (sản phẩm nhiều danh mục được tính cho mỗi danh mục — ghi chú trên UI) |
| `GET /stats/payment-providers` | `from`, `to` | `[{ provider, count, amount }]` từ `PaymentTransactions` (`Status = Success`) |
| `GET /stats/order-types` | `from`, `to` | `[{ type: "pickup"\|"delivery", count, revenue }]` |
| `GET /stats/new-users` | `from`, `to`, `groupBy` | `[{ period, count }]` |
| `GET /stats/low-stock` | `threshold` (mặc định 5), `limit` | `[{ productId, name, colorId, colorName, totalQuantity }]` (tổng mọi kho, sản phẩm `active`) |
| `GET /stats/reviews` | `from`, `to` | `{ total, average, distribution: { "1": n, …, "5": n }, lowestRated: [{ productId, name, average, count }] }` |

### 5.3 Mẫu: `overview`

```csharp
// src/Admin/Application/Services/AdminStatisticsServiceImpl.cs (trích)
private static readonly TimeZoneInfo VnTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

/// <summary>Đổi [from, to] (ngày giờ VN, bao gồm cả ngày to) sang [startUtc, endUtc).</summary>
private static (DateTime startUtc, DateTime endUtc) ToUtcRange(DateOnly from, DateOnly to)
{
    var start = TimeZoneInfo.ConvertTimeToUtc(from.ToDateTime(TimeOnly.MinValue), VnTz);
    var end = TimeZoneInfo.ConvertTimeToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue), VnTz);
    return (DateTime.SpecifyKind(start, DateTimeKind.Utc), DateTime.SpecifyKind(end, DateTimeKind.Utc));
}

private async Task<(double revenue, int orders, int cancelled, int total)> SummarizeOrders(DateTime startUtc, DateTime endUtc)
{
    var inRange = _context.Orders.AsNoTracking().Where(o => o.CreateDate >= startUtc && o.CreateDate < endUtc);
    var total = await inRange.CountAsync();
    var cancelled = await inRange.CountAsync(o => o.Status == OrderStatuses.Cancelled);
    var revenueOrders = inRange.Where(o => OrderStatuses.Revenue.Contains(o.Status));
    var orders = await revenueOrders.CountAsync();
    var goods = await revenueOrders.SelectMany(o => o.OrderDetails).SumAsync(d => (double?)d.Price * d.Quantity) ?? 0;
    var shipping = await revenueOrders.SelectMany(o => o.Fee.FeeDetails).SumAsync(f => (long?)f.Value) ?? 0;
    return (goods + shipping, orders, cancelled, total);
}

public async Task<StatsOverviewView> GetOverview(DateOnly from, DateOnly to)
{
    return (await _cache.GetOrCreateAsync($"stats:overview:{from}:{to}", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        var (start, end) = ToUtcRange(from, to);
        var length = end - start;
        var (prevStart, prevEnd) = (start - length, start);

        var cur = await SummarizeOrders(start, end);
        var prev = await SummarizeOrders(prevStart, prevEnd);
        var newCustomers = await _context.Users.CountAsync(u => u.CreateDate >= start && u.CreateDate < end);
        var prevNewCustomers = await _context.Users.CountAsync(u => u.CreateDate >= prevStart && u.CreateDate < prevEnd);

        return new StatsOverviewView(
            Revenue: cur.revenue,
            Orders: cur.total,
            PaidOrders: cur.orders,
            AverageOrderValue: cur.orders == 0 ? 0 : cur.revenue / cur.orders,
            NewCustomers: newCustomers,
            CancelledRate: cur.total == 0 ? 0 : (double)cur.cancelled / cur.total,
            RevenueChangePct: ChangePct(cur.revenue, prev.revenue),
            OrdersChangePct: ChangePct(cur.total, prev.total),
            NewCustomersChangePct: ChangePct(newCustomers, prevNewCustomers));
    }))!;
}

private static double? ChangePct(double current, double previous)
    => previous == 0 ? null : Math.Round((current - previous) / previous * 100, 1);
```

`DbContext` không an toàn khi chạy song song: các truy vấn trên chạy tuần tự (`await` từng cái), không `Task.WhenAll`.

### 5.4 Mẫu: `revenue` theo kỳ (SQL thuần)

EF Core không dịch được `GroupBy` theo `date_trunc` + múi giờ, nên dùng `SqlQuery` với DTO không khoá:

```csharp
public record RevenuePointRow(DateTime Period, double Revenue, int Orders);   // kết quả thô từ SQL
public record RevenuePointView(string Period, double Revenue, int Orders);    // trả API

public async Task<List<RevenuePointView>> GetRevenue(DateOnly from, DateOnly to, string groupBy)
{
    // Chỉ nhận whitelist; giá trị đi vào SQL dưới dạng tham số nên không bị injection, whitelist để báo lỗi rõ.
    if (groupBy is not ("day" or "week" or "month")) throw new BadRequestException("groupBy phải là day, week hoặc month.");
    var (start, end) = ToUtcRange(from, to);
    var statuses = OrderStatuses.Revenue.ToArray();

    var rows = await _context.Database.SqlQuery<RevenuePointRow>($"""
        SELECT date_trunc({groupBy}, o."CreateDate" AT TIME ZONE 'Asia/Ho_Chi_Minh') AS "Period",
               COALESCE(SUM(d.goods), 0) + COALESCE(SUM(f.shipping), 0)            AS "Revenue",
               COUNT(*)::int                                                         AS "Orders"
        FROM "Orders" o
        LEFT JOIN (SELECT "OrderId", SUM("Price" * "Quantity") AS goods
                   FROM "OrderDetails" GROUP BY "OrderId") d ON d."OrderId" = o."Id"
        LEFT JOIN (SELECT "FeeId", SUM("Value")::float8 AS shipping
                   FROM "FeeDetails" GROUP BY "FeeId") f ON f."FeeId" = o."FeeId"
        WHERE o."CreateDate" >= {start} AND o."CreateDate" < {end}
          AND o."Status" = ANY({statuses})
        GROUP BY 1
        ORDER BY 1
        """).ToListAsync();

    // Lấp kỳ trống để biểu đồ liền mạch.
    var byPeriod = rows.ToDictionary(r => DateOnly.FromDateTime(r.Period));
    var result = new List<RevenuePointView>();
    for (var p = AlignToPeriod(from, groupBy); p <= to; p = NextPeriod(p, groupBy))
    {
        var row = byPeriod.GetValueOrDefault(p);
        result.Add(new RevenuePointView(p.ToString("yyyy-MM-dd"), row?.Revenue ?? 0, row?.Orders ?? 0));
    }
    return result;
}
// AlignToPeriod: day → chính nó; week → thứ Hai (date_trunc('week') của Postgres bắt đầu thứ Hai); month → ngày 1.
// NextPeriod: +1 ngày / +7 ngày / +1 tháng.
```

Tên bảng/cột là tên DbSet/property nên phải đặt trong ngoặc kép (PascalCase). `OrderDetails."OrderId"` là FK ẩn
(nullable) do EF sinh từ `Order.OrderDetails`, đã có index; đối chiếu `Migrations/ApplicationDbContextModelSnapshot.cs`
khi đổi schema.

### 5.5 Mẫu: `top-products` (LINQ)

`OrderDetail` không có navigation về `Order` (chỉ có FK ẩn `OrderId` sinh từ `Order.OrderDetails`), nên đi từ `Orders`:

```csharp
var (start, end) = ToUtcRange(from, to);
var query = _context.Orders.AsNoTracking()
    .Where(o => o.CreateDate >= start && o.CreateDate < end && OrderStatuses.Revenue.Contains(o.Status))
    .SelectMany(o => o.OrderDetails)
    .GroupBy(d => new { d.ProductOrderDetailId, d.Product!.Name, d.Product.ImageUrl })
    .Select(g => new TopProductView(g.Key.ProductOrderDetailId, g.Key.Name, g.Key.ImageUrl,
                                    g.Sum(d => d.Quantity), g.Sum(d => d.Price * d.Quantity)));
query = orderBy == "revenue" ? query.OrderByDescending(x => x.Revenue) : query.OrderByDescending(x => x.Quantity);
return await query.Take(Math.Clamp(limit, 1, 50)).ToListAsync();
```

### 5.6 Hiệu năng

Migration `Admin_StatsIndexes`:
- `Orders (Status, CreateDate)`
- `Orders (CreateDate)`
- `AspNetUsers (CreateDate)`
- `PaymentTransactions (CreateDate)`

Khi dữ liệu lớn: thêm bảng tổng hợp theo ngày `DailySalesSummary` do Hangfire job (`stats-daily-rollup`, mỗi giờ) cập
nhật cho ngày hôm nay và hôm qua; endpoint đọc bảng này cho các ngày đã qua. Chưa cần ở quy mô hiện tại.

---

## 6. Mẫu một tính năng hoàn chỉnh: đổi trạng thái đơn hàng

Làm theo đúng thứ tự này cho mọi tính năng admin.

**1. Domain — luật trên entity** (`src/Orders/Domain/Entities/Order.cs`, trong region trạng thái):

```csharp
/// <summary>Paid → Shipping (chỉ đơn giao hàng).</summary>
public void Ship(string userId)
{
    if (Status != OrderStatuses.Paid || Type != OrderTypes.Delivery)
        throw new InvalidOperationException($"Không thể chuyển sang giao hàng từ trạng thái: {Status}");
    Status = OrderStatuses.Shipping;
    UpdateDate = DateTime.UtcNow;
    UpdateBy = userId;
}

/// <summary>Shipping → Completed (giao hàng), Paid/Pending → Completed (nhận tại cửa hàng, trả tại quầy).</summary>
public void Complete(string userId)
{
    var allowed = Type == OrderTypes.Delivery
        ? Status == OrderStatuses.Shipping
        : Status is OrderStatuses.Paid or OrderStatuses.Pending;
    if (!allowed) throw new InvalidOperationException($"Không thể hoàn tất đơn ở trạng thái: {Status}");
    Status = OrderStatuses.Completed;
    UpdateDate = DateTime.UtcNow;
    UpdateBy = userId;
}
```

(Kiểm tra tên hằng trong `src/Shared/Constants/OrderTypes.cs`.)

**2. DTO + validator** (`src/Orders/Application/DTOs/Admin/UpdateOrderStatusRequest.cs`, `Validation/Admin/`):

```csharp
namespace cellPhoneS_backend.DTOs.Requests;

public record UpdateOrderStatusRequest(string Status, string? Note);
```

```csharp
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;

namespace cellPhoneS_backend.Validation;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status).Must(s => s is OrderStatuses.Shipping or OrderStatuses.Completed)
            .WithMessage("Trạng thái chỉ có thể là Shipping hoặc Completed.");
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
```

Hiện **không validator nào được chạy**: project chỉ tham chiếu gói `FluentValidation` (không có tích hợp ASP.NET),
không đăng ký validator vào DI và không gọi `Validate` ở đâu. Phase 1 thêm gói
`FluentValidation.DependencyInjectionExtensions`, `services.AddValidatorsFromAssemblyContaining<LoginDTOValidator>()`
và một action filter global: với mỗi argument có `IValidator<T>` thì `ValidateAsync`, lỗi → 400
`ApiResponse<object?>(<lỗi đầu tiên>, <danh sách lỗi>)`. Bật cho cả validator storefront đang có thì cần thử lại các
form đăng ký/đặt hàng.

**3. Service** (`src/Orders/Application/Services/Admin/`):

```csharp
namespace cellPhoneS_backend.Services.Interface;

public interface IAdminOrderService
{
    Task<PagedResult<AdminOrderView>> GetOrders(AdminOrderFilter filter);
    Task<AdminOrderView> UpdateStatus(long orderId, UpdateOrderStatusRequest request, string adminId);
    Task Cancel(long orderId, string reason, string adminId);
    Task<string> SyncPayment(long orderId);
}
```

```csharp
public async Task<AdminOrderView> UpdateStatus(long orderId, UpdateOrderStatusRequest request, string adminId)
{
    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId)
        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");
    try
    {
        if (request.Status == OrderStatuses.Shipping) order.Ship(adminId);
        else order.Complete(adminId);
    }
    catch (InvalidOperationException ex)
    {
        throw new ConflictException(ex.Message);   // sai trạng thái → 409
    }
    await _context.SaveChangesAsync();
    return await _adminQueryService.GetOrder(orderId);  // tái dùng projection chi tiết đã có
}

public async Task<string> SyncPayment(long orderId)
{
    await _orderService.HandlePaymentCallback(orderId);   // hỏi gateway rồi MarkOrderPaid (idempotent)
    return (await _context.Orders.AsNoTracking().FirstAsync(o => o.Id == orderId)).Status;
}
```

**4. Controller** (`src/Orders/Api/Controllers/Admin/AdminOrderController.cs`):

```csharp
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/orders")]
    [ApiController]
    [RequirePermission(Permissions.OrderRead)]
    public class AdminOrderController : BaseController
    {
        private readonly IAdminOrderService _adminOrderService;

        public AdminOrderController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        // PATCH: api/admin/orders/{orderId}/status
        [HttpPatch("{orderId}/status")]
        [RequirePermission(Permissions.OrderUpdateStatus)]
        public async Task<ActionResult<ApiResponse<AdminOrderView>>> UpdateStatus(long orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            var result = await _adminOrderService.UpdateStatus(orderId, request, CurrentUserId);
            return Ok(new ApiResponse<AdminOrderView>("Cập nhật trạng thái đơn hàng thành công", result));
        }

        // POST: api/admin/orders/{orderId}/sync-payment
        [HttpPost("{orderId}/sync-payment")]
        [RequirePermission(Permissions.OrderUpdateStatus)]
        public async Task<ActionResult<ApiResponse<string>>> SyncPayment(long orderId)
        {
            var status = await _adminOrderService.SyncPayment(orderId);
            return Ok(new ApiResponse<string>("Đã đồng bộ trạng thái thanh toán", status));
        }
    }
}
```

`GET /api/admin/orders` và `/orders/{id}` hiện ở `AdminQueryController`: hai controller cùng prefix vẫn chạy được
(route khác nhau); khi tiện thì dời hai GET sang `AdminOrderController`.

**5. DI** — `ServiceCollectionExtensions.AddCustomServices()`, trong khối của module:

```csharp
// --- Admin: Orders ---
services.AddScoped<IAdminOrderService, AdminOrderServiceImpl>();
```

**6. RouteConfig** — không cần sửa (đã nằm dưới `/api/admin/*`).

**7. File `.http`** — `http/admin/orders.http`:

```http
@host = http://localhost:5169
@token = <access token của tài khoản có order.update-status>

PATCH {{host}}/api/admin/orders/15/status
Authorization: Bearer {{token}}
Content-Type: application/json

{ "status": "Shipping", "note": "Đã bàn giao GHTK" }

###
POST {{host}}/api/admin/orders/15/sync-payment
Authorization: Bearer {{token}}
```

**8. Kiểm tra tay:** tài khoản STAFF gọi được `PATCH status`, bị 403 với `POST /cancel`; tài khoản USER bị 403 ở mọi
route `/api/admin/*`; đơn `Pending` delivery → `Shipping` trả 409; bảng `AuditLog` có dòng tương ứng.

---

## 7. Lộ trình theo phase

| Phase | Nội dung | Phụ thuộc | Song song |
|---|---|---|---|
| **0. Vá bảo mật** | Dời product write sang `AdminProductController`; khoá `/api/user/*` (chỉ còn `me`), không trả entity `User`; login/refresh chặn user không `active`; kiểm tra blacklist trong `OnTokenValidated` | — | Làm ngay, một PR nhỏ. Frontend admin phải đổi URL sản phẩm (`POST/PUT/DELETE admin/products`) cùng lúc |
| **1. Nền tảng** | `Permissions`, `RoleNames`, seed role + permission, `IPermissionService`, `RequirePermission`, sửa `RoutePolicy`/middleware, `GET /me`, API roles/permissions, `AuditLog` + filter, `PagingExtensions`/`AdminListQuery`, `BaseController.CurrentUserId`, OpenAPI dev, chạy validator FluentValidation (mục 6 bước 2), gắn `[RequirePermission]` cho mọi controller admin đang có | Phase 0 | Một người làm (đụng file chung). Có thể tách: (a) permission + roles, (b) audit + paging + OpenAPI |
| **2. CRUD đầy đủ** | 4.2 Users · 4.3 Products · 4.4 Catalog list admin · 4.5 Inventory · 4.6 Orders/Transactions · 4.7 Reviews | Phase 1 | **Mỗi module một nhánh**, độc lập nhau (chỉ chạm DI + migration riêng) |
| **3. Thống kê** | `AdminStatisticsController` + service (mục 5), index migration, cache | Phase 1 (permission). Không cần Phase 2 | Song song với Phase 2 |
| **4. Hoàn thiện** | Xuất CSV (`GET /orders/export`, `/stats/revenue/export`), dự án test `cellPhoneS_backend.Tests` (xUnit + `WebApplicationFactory` + Postgres Testcontainers) cho phân quyền và luật trạng thái, rollup job nếu cần | Phase 2–3 | |

Song song với frontend: sau Phase 1, frontend làm `/api/admin/me` + guard theo permission; mỗi tính năng Phase 2–3
chỉ cần hợp đồng (DTO + route, mục 1.3) đã merge là dựng được giao diện.

---

## 8. Checklist khi thêm một API admin

- [ ] Route bắt đầu `api/admin/`, controller `Admin<Name>Controller` trong `src/<Module>/Api/Controllers/Admin/`.
- [ ] `[RequirePermission]` trên controller (quyền đọc) và trên từng action ghi; permission mới thì thêm vào `Permissions` + `All` + bảng 2.3 + seed.
- [ ] DTO riêng trong `Application/DTOs/Admin/`, không trả entity; request có validator.
- [ ] Luật trạng thái trên entity; service ném exception nghiệp vụ, không trả `null`/`bool` để báo lỗi.
- [ ] List dùng `AdminListQuery` + `ToPagedResultAsync`, sort theo whitelist, `pageSize ≤ 100`.
- [ ] Ghi đủ `UpdateBy/UpdateDate`; xoá là xoá mềm; làm mới cache liên quan.
- [ ] Đăng ký DI trong khối của module; migration tên `Admin_<Feature>`.
- [ ] File `http/admin/<resource>.http`.
- [ ] Thử với 3 tài khoản: có quyền (2xx), STAFF/thiếu quyền (403), USER (403).
- [ ] Cập nhật bảng endpoint ở mục 4 (trạng thái Mới → Có).
