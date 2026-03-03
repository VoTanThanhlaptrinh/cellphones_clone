using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface BrandService
{
    // Admin operations
    Task<ServiceResult<long>> CreateBrand(CreateBrandRequest request, string userId);
    Task<ServiceResult<string>> UpdateBrand(long brandId, UpdateBrandRequest request, string userId);
    Task<ServiceResult<string>> DeleteBrand(long brandId, string userId);
    
    // Public/User operations
    Task<ServiceResult<PagedResult<BrandResponse>>> GetAllBrands(int pageIndex, int pageSize, string? status = "active");
    Task<ServiceResult<List<BrandResponse>>> GetBrandsByCategory(long categoryId);
    Task<ServiceResult<BrandResponse>> GetBrandById(long brandId);
}
