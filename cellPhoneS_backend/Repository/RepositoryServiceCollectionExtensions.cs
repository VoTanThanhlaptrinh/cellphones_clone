using Microsoft.Extensions.DependencyInjection;
using cellphones_backend.Repositories;

namespace cellphones_backend.Repositories;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartDetailRepository, CartDetailRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IColorRepository, ColorRepository>();
        services.AddScoped<ICommitmentRepository, CommitmentRepository>();
        services.AddScoped<ICriterionRepository, CriterionRepository>();
        services.AddScoped<ICriterionDetailRepository, CriterionDetailRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IInfoRepository, InfoRepository>();
        services.AddScoped<IOauth2Repository, Oauth2Repository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductSpecificationRepository, ProductSpecificationRepository>();
        services.AddScoped<ISeriesRepository, SeriesRepository>();
        services.AddScoped<ISpecificationRepository, SpecificationRepository>();
        services.AddScoped<ISpecificationDetailRepository, SpecificationDetailRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IStoreHouseRepository, StoreHouseRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IGrantRoleRepository, GrantRoleRepository>();
        services.AddScoped<IDemandRepository, DemandRepository>();
        services.AddScoped<IDemandImageRepository, DemandImageRepository>();
        services.AddScoped<ICategoryProductRepository, CategoryProductRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        return services;
    }
}
