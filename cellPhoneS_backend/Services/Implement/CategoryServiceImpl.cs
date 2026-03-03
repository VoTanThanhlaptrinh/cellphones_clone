
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

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
    
    public async Task<ServiceResult<long>> CreateCategory(CreateCategoryRequest request, string userId)
    {
        var existingNames = await _categoryRepository.FindAsync(c => c.Name == request.Name && c.Status != "deleted");
        if (existingNames.Any())
            return ServiceResult<long>.Fail("Category with same name already exists");

        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value);
            if (parent == null || parent.Status == "deleted")
                return ServiceResult<long>.Fail("Parent category not found");
        }

        var category = new Category
        {
            Name = request.Name,
            SlugName = SlugHelper.GenerateSlug(request.Name),
            ParentCategoryId = request.ParentCategoryId,
            Status = request.Status ?? "active",
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreateBy = userId,
            UpdateBy = userId
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return ServiceResult<long>.Success(category.Id, "Category created successfully");
    }

    public async Task<ServiceResult<string>> UpdateCategory(long categoryId, UpdateCategoryRequest request, string userId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.Status == "deleted")
            return ServiceResult<string>.Fail("Category not found");

        if (!string.IsNullOrEmpty(request.Name) && request.Name != category.Name)
        {
            var existingNames = await _categoryRepository.FindAsync(c => c.Name == request.Name && c.Id != categoryId && c.Status != "deleted");
            if (existingNames.Any())
                return ServiceResult<string>.Fail("Category with same name already exists");

            category.Name = request.Name;
            category.SlugName = SlugHelper.GenerateSlug(request.Name);
        }

        if (request.ParentCategoryId != category.ParentCategoryId)
        {
            if (request.ParentCategoryId == 0)
            {
                category.ParentCategoryId = null;
            }
            else if (request.ParentCategoryId.HasValue)
            {
                if (request.ParentCategoryId.Value == categoryId)
                    return ServiceResult<string>.Fail("Category cannot be its own parent");

                var parent = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value);
                if (parent == null || parent.Status == "deleted")
                    return ServiceResult<string>.Fail("Parent category not found");

                category.ParentCategoryId = request.ParentCategoryId.Value;
            }
        }

        if (request.Status != null)
        {
            category.Status = request.Status;
        }

        category.UpdateDate = DateTime.Now;
        category.UpdateBy = userId;

        await _categoryRepository.UpdateAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("", "Category updated successfully");
    }

    public async Task<ServiceResult<string>> DeleteCategory(long categoryId, string userId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.Status == "deleted")
            return ServiceResult<string>.Fail("Category not found");

        category.Status = "deleted";
        category.UpdateDate = DateTime.Now;
        category.UpdateBy = userId;

        await _categoryRepository.UpdateAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("","Category deleted successfully");
    }

    public async Task<ServiceResult<PagedResult<CategoryResponse>>> GetAllCategories(int pageIndex, int pageSize, string? status = "active")
    {
        var query = await _categoryRepository.FindAsync(c => status == null || c.Status == status);
        
        var categories = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = query.Count();

        if (!categories.Any())
            return ServiceResult<PagedResult<CategoryResponse>>.Fail("No categories found");

        var resultItems = categories.Select(c => new CategoryResponse(c.Id, c.Name, c.SlugName, null, null, null)).ToList();
        var pagedResult = new PagedResult<CategoryResponse>(resultItems, totalCount, pageIndex, pageSize);

        return ServiceResult<PagedResult<CategoryResponse>>.Success(pagedResult, "Success");
    }

    public async Task<ServiceResult<CategoryDetailResponse>> GetCategoryDetail(long categoryId)
    {
        var res =  await _categoryRepository.GetCategoryDetail(categoryId, size);
        if(res == null)
            return ServiceResult<CategoryDetailResponse>.Fail("No category found for the specified category");
            
        var response = new CategoryDetailResponse(res.Id, res.Name, res.SlugName, res.Demands?.Select(d => new DemandResponse(d.Id, d.Name, d.SlugName)).ToList(), res.Brands?.Select(b => new BrandResponse(b.Id, b.Name, b.SlugName)).ToList(), res.Products, res.total);
        return ServiceResult<CategoryDetailResponse>.Success(response, "Success");
    }

    public async Task<ServiceResult<CategoryDetailResponse>> GetCategoryDetailBySlug(string slugName)
    {
        var res =  await _categoryRepository.GetCategoryDetailBySlug(slugName, size);
        if(res == null)
            return ServiceResult<CategoryDetailResponse>.Fail("No category found for the specified category");
            
        var response = new CategoryDetailResponse(res.Id, res.Name, res.SlugName, res.Demands?.Select(d => new DemandResponse(d.Id, d.Name, d.SlugName)).ToList(), res.Brands?.Select(b => new BrandResponse(b.Id, b.Name, b.SlugName)).ToList(), res.Products, res.total);
        return ServiceResult<CategoryDetailResponse>.Success(response, "Success");
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
}
