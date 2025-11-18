using System;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;

namespace cellPhoneS_backend.Services.Implement;

public class CategoryServiceImpl : CategoryService
{
    
    public Task<ServiceResult<string>> AddCategory(CategoryRequest categoryRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> DeleteCategory(long categoryId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<ProductView>>> GetProductOfCategory(long categoryId, int size, int page)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> UpdateCategory(CategoryRequest categoryRequest)
    {
        throw new NotImplementedException();
    }
}
