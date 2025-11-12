using System;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.J2O;

namespace cellPhoneS_backend.Services;

public interface CategoryService
{
    public ApiResponse<string> AddCategory(CategoryRequest categoryRequest);
    public ApiResponse<string> UpdateCategory(CategoryRequest categoryRequest);
    public ApiResponse<string> DeleteCategory(long categoryId);
    public ApiResponse<List<ProductView>> GetProductOfCategory(long categoryId, int size, int page);

}
