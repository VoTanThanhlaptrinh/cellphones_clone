using cellphones_backend.Services;
using cellphones_backend.Services.Implement;
using cellphones_backend.Services.Interface;
using cellPhoneS_backend.Services.Implement;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService, AuthServiceImpl>();
        services.AddScoped<UserService, UserServiceImpl>();
        services.AddScoped<AzuriteService, AzuriteServiceImpl>();
        services.AddScoped<ImageDownloaderService, ImageDownloaderServiceImpl>();
        services.AddScoped<CloneService, CloneServiceImpl>();
        services.AddScoped<JwtTokenService, JwtTokenServiceImpl>();
        services.AddScoped<InitService, InitServiceImpl>();
        services.AddScoped<ProductService, ProductServiceImpl>();
        services.AddScoped<ShippingFeeService, ShippingFeeServiceImpl>();
        services.AddScoped<CategoryService, CategoryServiceImpl>();
        services.AddScoped<CartService, CartServiceImpl>();
        services.AddScoped<CartDetailService, CartDetailServiceImpl>();
        services.AddScoped<ProductSearchService, ProductSearchServiceImpl>();
        services.AddScoped<IJwtBlacklistService, JwtBlacklistServiceImpl>();
        services.AddScoped<OrderService, OrderServiceImpl>();
        services.AddScoped<CacheService>();
        services.AddScoped<UploadImage2Cloud>();
        services.AddScoped<GhtkApiService>();
        // Domain entity services
        services.AddScoped<BrandService, BrandServiceImpl>();
        services.AddScoped<DemandService, DemandServiceImpl>();
        services.AddScoped<SeriesService, SeriesServiceImpl>();
        services.AddScoped<StoreHouseService, StoreHouseServiceImpl>();
        services.AddScoped<ImageService, ImageServiceImpl>();
        services.AddScoped<SpecificationService, SpecificationServiceImpl>();
        services.AddScoped<CommitmentService, CommitmentServiceImpl>();
        services.AddScoped<IStoreService, StoreServiceImpl>();
        services.AddScoped<IVietnamLocationService, VietnamLocationServiceImpl>();
        services.AddScoped<ShippingFeeService, ShippingFeeServiceImpl>();
        services.AddScoped<IEmailService, EmailServiceImpl>();
        // --- Admin: cross-module (permissions, roles, audit) ---
        services.AddScoped<IPermissionService, PermissionServiceImpl>();
        services.AddScoped<IAdminRoleService, AdminRoleServiceImpl>();
        services.AddScoped<IAuditLogService, AuditLogServiceImpl>();
        // --- Admin: Account (users) ---
        services.AddScoped<IAdminUserService, AdminUserServiceImpl>();
        // --- Admin: Orders & Payments ---
        services.AddScoped<IAdminOrderService, AdminOrderServiceImpl>();
        services.AddScoped<IAdminTransactionService, AdminTransactionServiceImpl>();
        // --- Admin: Inventory ---
        services.AddScoped<IAdminInventoryService, AdminInventoryServiceImpl>();
        // --- Admin: Catalog ---
        services.AddScoped<IAdminCatalogQueryService, AdminCatalogQueryServiceImpl>();
        // --- Admin: Products ---
        services.AddScoped<IAdminProductQueryService, AdminProductQueryServiceImpl>();
        // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
        services.AddScoped<IReviewService, ReviewServiceImpl>();
        // === END NEW FEATURE: PRODUCT REVIEWS ===
        return services;
    }
}
