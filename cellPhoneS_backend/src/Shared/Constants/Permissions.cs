namespace cellPhoneS_backend.Constants;

/// <summary>
/// Danh mục permission của khu quản trị, dạng "&lt;resource&gt;.&lt;action&gt;". Role chỉ được gán các giá trị có trong <see cref="All"/>.
/// Thêm permission mới: thêm hằng số (trong khối của module), thêm vào <see cref="All"/>, cập nhật bảng ở docs/ADMIN_API_GUIDE.md
/// và <c>AdminSeeder.DefaultPermissions</c>.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    /// <summary>Được vào khu /api/admin/* (mọi role quản trị phải có).</summary>
    public const string AdminAccess = "admin.access";

    // --- Catalog (danh mục, thương hiệu, dòng, nhu cầu) ---
    public const string CatalogRead = "catalog.read";
    public const string CatalogWrite = "catalog.write";

    // --- Products (sản phẩm, thông số, cam kết) ---
    public const string ProductRead = "product.read";
    public const string ProductWrite = "product.write";
    public const string ProductDelete = "product.delete";

    // --- Media ---
    public const string MediaWrite = "media.write";

    // --- Stores (kho, tồn kho) ---
    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";

    // --- Orders & Payments ---
    public const string OrderRead = "order.read";
    public const string OrderUpdateStatus = "order.update-status";
    public const string OrderCancel = "order.cancel";
    public const string TransactionRead = "transaction.read";

    // --- Account ---
    public const string UserRead = "user.read";
    public const string UserLock = "user.lock";
    public const string UserAssignRole = "user.assign-role";
    public const string RoleManage = "role.manage";

    // --- Reviews ---
    public const string ReviewModerate = "review.moderate";

    // --- Admin (xuyên module) ---
    public const string StatsView = "stats.view";
    public const string AuditRead = "audit.read";

    public static readonly IReadOnlyList<string> All = new[]
    {
        AdminAccess,
        // Catalog
        CatalogRead, CatalogWrite,
        // Products
        ProductRead, ProductWrite, ProductDelete,
        // Media
        MediaWrite,
        // Stores
        InventoryRead, InventoryWrite,
        // Orders & Payments
        OrderRead, OrderUpdateStatus, OrderCancel, TransactionRead,
        // Account
        UserRead, UserLock, UserAssignRole, RoleManage,
        // Reviews
        ReviewModerate,
        // Admin
        StatsView, AuditRead
    };
}
