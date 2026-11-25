
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;


namespace cellPhoneS_backend.Services;

public interface CategoryService
{
    // Admin operations
    Task<long> CreateCategory(CreateCategoryRequest request, string userId);
    Task UpdateCategory(long categoryId, UpdateCategoryRequest request, string userId);
    Task DeleteCategory(long categoryId, string userId);
    Task RestoreCategory(long categoryId, string userId);
    
    // Public/User operations
    Task<PagedResult<CategoryResponse>> GetAllCategories(int pageIndex, int pageSize, string? status = "active");
    Task<List<ProductView>> GetProductOfCategory(long categoryId, int page);
    Task<CategoryDetailResponse> GetCategoryDetail(long category);
    Task<CategoryDetailResponse> GetCategoryDetailBySlug(string slugName);
}