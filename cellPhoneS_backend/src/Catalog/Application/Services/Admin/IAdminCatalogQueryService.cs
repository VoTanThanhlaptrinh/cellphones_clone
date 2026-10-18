using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Đọc danh mục / thương hiệu / dòng / nhu cầu cho trang quản trị (mọi trạng thái, lọc, phân trang).</summary>
public interface IAdminCatalogQueryService
{
    Task<PagedResult<AdminCategoryView>> GetCategories(AdminCatalogFilter filter);
    Task<AdminCategoryView> GetCategory(long id);
    Task<PagedResult<AdminBrandView>> GetBrands(AdminCatalogFilter filter);
    Task<AdminBrandView> GetBrand(long id);
    Task<PagedResult<AdminSeriesView>> GetSeries(AdminCatalogFilter filter);
    Task<AdminSeriesView> GetSeriesById(long id);
    Task<PagedResult<AdminDemandView>> GetDemands(AdminCatalogFilter filter);
    Task<AdminDemandView> GetDemand(long id);
}
