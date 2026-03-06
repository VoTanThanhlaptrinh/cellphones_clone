
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;


namespace cellPhoneS_backend.Services;

public interface CategoryService
{
    // Admin operations
    Task<ServiceResult<long>> CreateCategory(CreateCategoryRequest request, string userId);
    Task<ServiceResult<string>> UpdateCategory(long categoryId, UpdateCategoryRequest request, string userId);
    Task<ServiceResult<string>> DeleteCategory(long categoryId, string userId);
    
    // Public/User operations
    Task<ServiceResult<PagedResult<CategoryResponse>>> GetAllCategories(int pageIndex, int pageSize, string? status = "active");
    Task<ServiceResult<List<ProductView>>> GetProductOfCategory(long categoryId, int page);
    Task<ServiceResult<CategoryDetailResponse>> GetCategoryDetail(long category);
    Task<ServiceResult<CategoryDetailResponse>> GetCategoryDetailBySlug(string slugName);
}