using System;
using cellphones_backend.Services;
using cellphones_backend.Services.Implement;
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
        services.AddScoped<SeriesService, SeriesServiceImpl>();
        services.AddScoped<ImageService, ImageServiceImpl>();
        services.AddScoped<SpecificationService, SpecificationServiceImpl>();
        services.AddScoped<CommitmentService, CommitmentServiceImpl>();
        services.AddScoped<IStoreService, StoreServiceImpl>();

        return services;
    }
}
