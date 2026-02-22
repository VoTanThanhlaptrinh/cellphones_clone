using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface BrandService
{
    // Admin operations
    Task<ServiceResult<long>> CreateBrand(CreateBrandRequest request, string userId);
    Task<ServiceResult<string>> UpdateBrand(long brandId, CreateBrandRequest request, string userId);
    Task<ServiceResult<string>> DeleteBrand(long brandId, string userId);
    
    // Public/User operations
    Task<ServiceResult<List<BrandView>>> GetAllBrands();
    Task<ServiceResult<List<BrandView>>> GetBrandsByCategory(long categoryId);
    Task<ServiceResult<BrandView>> GetBrandById(long brandId);
}
