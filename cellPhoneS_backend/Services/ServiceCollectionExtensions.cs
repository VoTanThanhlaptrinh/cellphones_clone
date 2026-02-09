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
        services.AddScoped<ProductSearchService, ProductSearchServiceImpl>();
        services.AddScoped<IJwtBlacklistService, JwtBlacklistServiceImpl>();
        return services;
    }
}
