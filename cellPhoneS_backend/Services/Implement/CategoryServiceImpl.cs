using System;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;

namespace cellPhoneS_backend.Services.Implement;

public class CategoryServiceImpl : CategoryService
{
    
    public ApiResponse<string> AddCategory(CategoryRequest categoryRequest)
    {
        throw new NotImplementedException();
    }

    public ApiResponse<string> DeleteCategory(long categoryId)
    {
        throw new NotImplementedException();
    }

    public ApiResponse<List<ProductView>> GetProductOfCategory(long categoryId, int size, int page)
    {
        throw new NotImplementedException();
    }

    public ApiResponse<string> UpdateCategory(CategoryRequest categoryRequest)
    {
        throw new NotImplementedException();
    }
}
