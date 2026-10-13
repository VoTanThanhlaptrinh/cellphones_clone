
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
    
    public async Task<long> CreateCategory(CreateCategoryRequest request, string userId)
    {
        var existingNames = await _categoryRepository.FindAsync(c => c.Name == request.Name && c.Status != "deleted");
        if (existingNames.Any())
            throw new ConflictException("Category with same name already exists");

        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value);
            if (parent == null || parent.Status == "deleted")
                throw new NotFoundException("Parent category not found");
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

        return category.Id;
    }

    public async Task UpdateCategory(long categoryId, UpdateCategoryRequest request, string userId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.Status == "deleted")
            throw new NotFoundException("Category not found");

        if (!string.IsNullOrEmpty(request.Name) && request.Name != category.Name)
        {
            var existingNames = await _categoryRepository.FindAsync(c => c.Name == request.Name && c.Id != categoryId && c.Status != "deleted");
            if (existingNames.Any())
                throw new ConflictException("Category with same name already exists");

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
                    throw new BadRequestException("Category cannot be its own parent");

                var parent = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value);
                if (parent == null || parent.Status == "deleted")
                    throw new NotFoundException("Parent category not found");

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
    }

    public async Task DeleteCategory(long categoryId, string userId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.Status == "deleted")
            throw new NotFoundException("Category not found");

        category.Status = "deleted";
        category.UpdateDate = DateTime.Now;
        category.UpdateBy = userId;

        await _categoryRepository.UpdateAsync(category);
        await _categoryRepository.SaveChangesAsync();
    }

    public async Task<PagedResult<CategoryResponse>> GetAllCategories(int pageIndex, int pageSize, string? status = "active")
    {
        var query = await _categoryRepository.FindAsync(c => status == null || c.Status == status);
        
        var categories = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = query.Count();

        var resultItems = categories.Select(c => new CategoryResponse(c.Id, c.Name, c.SlugName, null, null, null)).ToList();
        var pagedResult = new PagedResult<CategoryResponse>(resultItems, totalCount, pageIndex, pageSize);

        return pagedResult;
    }

    public async Task<CategoryDetailResponse> GetCategoryDetail(long categoryId)
    {
        var res =  await _categoryRepository.GetCategoryDetail(categoryId, size);
        if(res == null)
            throw new NotFoundException("No category found for the specified category");
            
        var response = new CategoryDetailResponse(res.Id, res.Name, res.SlugName, res.Demands?.Select(d => new DemandResponse(d.Id, d.Name, d.SlugName)).ToList(), res.Brands?.Select(b => new BrandResponse(b.Id, b.Name, b.SlugName)).ToList(), res.Products, res.total);
        return response;
    }

    public async Task<CategoryDetailResponse> GetCategoryDetailBySlug(string slugName)
    {
        var res =  await _categoryRepository.GetCategoryDetailBySlug(slugName, size);
        if(res == null)
            throw new NotFoundException("No category found for the specified category");
            
        var response = new CategoryDetailResponse(res.Id, res.Name, res.SlugName, res.Demands?.Select(d => new DemandResponse(d.Id, d.Name, d.SlugName)).ToList(), res.Brands?.Select(b => new BrandResponse(b.Id, b.Name, b.SlugName)).ToList(), res.Products, res.total);
        return response;
    }

    public async Task<List<ProductView>> GetProductOfCategory(long categoryId, int page)
    {
        var products = await _categoryRepository.GetProductOfCategory(categoryId, size, page);
        return products ?? new List<ProductView>();
    }
}
