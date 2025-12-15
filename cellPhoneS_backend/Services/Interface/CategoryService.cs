using System;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.J2O;
using cellPhoneS_backend.Services;

namespace cellPhoneS_backend.Services;

public interface CategoryService
{
    Task<ServiceResult<string>> AddCategory(CategoryRequest categoryRequest);
    Task<ServiceResult<string>> UpdateCategory(CategoryRequest categoryRequest);
    Task<ServiceResult<string>> DeleteCategory(long categoryId);
    Task<ServiceResult<List<ProductView>>> GetProductOfCategory(long categoryId, int page);
    Task<ServiceResult<CategoryDetailView>> GetCategoryDetail(long category, int page);
}