using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellphones_backend.Services;

public interface BrandService
{
    // Admin operations
    Task<long> CreateBrand(CreateBrandRequest request, string userId);
    Task UpdateBrand(long brandId, UpdateBrandRequest request, string userId);
    Task DeleteBrand(long brandId, string userId);
    Task RestoreBrand(long brandId, string userId);
    
    // Public/User operations
    Task<PagedResult<BrandResponse>> GetAllBrands(int pageIndex, int pageSize, string? status = "active");
    Task<List<BrandResponse>> GetBrandsByCategory(long categoryId);
    Task<BrandResponse> GetBrandById(long brandId);
}
