using System;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.J2O;

namespace cellPhoneS_backend.Services.Implement;

public class CategoryServiceImpl : CategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IConfiguration _configuration;
    private int size;
    public CategoryServiceImpl(ICategoryRepository categoryRepository, IConfiguration configuration)
    {
        _categoryRepository = categoryRepository;
        _configuration = configuration;
        size = _configuration.GetValue<int>("DefaultSetting:PageSize");
    }
    
    public Task<ServiceResult<string>> AddCategory(CategoryRequest categoryRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> DeleteCategory(long categoryId)
    {
        throw new NotImplementedException();
    }

    public async  Task<ServiceResult<CategoryDetailView>> GetCategoryDetail(long categoryId, int page)
    {
        var res =  await _categoryRepository.GetCategoryDetail(categoryId, size ,page);
        if(res == null)
            return ServiceResult<CategoryDetailView>.Fail("No category found for the specified category");
        return ServiceResult<CategoryDetailView>.Success(res,"Success");
    }

    public async Task<ServiceResult<List<ProductView>>> GetProductOfCategory(long categoryId, int page)
    {
       var products = await _categoryRepository.GetProductOfCategory(categoryId, size, page);
       if (products == null || products.Count == 0)
       {
           return ServiceResult<List<ProductView>>.Fail("No products found for the specified category.");
       }
         return ServiceResult<List<ProductView>>.Success(products, "success");
            
    }

    public Task<ServiceResult<string>> UpdateCategory(CategoryRequest categoryRequest)
    {
        throw new NotImplementedException();
    }
}
