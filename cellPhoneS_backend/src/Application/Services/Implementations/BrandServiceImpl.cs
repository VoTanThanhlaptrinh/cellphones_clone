using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class BrandServiceImpl : BrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IImageRepository _imageRepository;

    public BrandServiceImpl(
        IBrandRepository brandRepository, 
        ICategoryRepository categoryRepository,
        IImageRepository imageRepository)
    {
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _imageRepository = imageRepository;
    }

    public async Task<ServiceResult<long>> CreateBrand(CreateBrandRequest request, string userId)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            return ServiceResult<long>.Fail("Category not found");

        if (request.ImageId.HasValue)
        {
            var image = await _imageRepository.GetByIdAsync(request.ImageId.Value);
            if (image == null)
                return ServiceResult<long>.Fail("Image not found");
        }

        var existingNames = await _brandRepository.FindAsync(b => b.Name == request.Name && b.CategoryId == request.CategoryId && b.Status != "deleted");
        if (existingNames.Any())
            return ServiceResult<long>.Fail("Brand with same name already exists in this category");

        var brand = new Brand
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            SlugName = SlugHelper.GenerateSlug(request.Name),
            ImageId = request.ImageId,
            Status = request.Status ?? "active",
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            CreateBy = userId,
            UpdateBy = userId
        };

        await _brandRepository.AddAsync(brand);
        await _brandRepository.SaveChangesAsync();

        return ServiceResult<long>.Success(brand.Id, "Brand created successfully");
    }

    public async Task<ServiceResult<string>> UpdateBrand(long brandId, UpdateBrandRequest request, string userId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.Status == "deleted")
            return ServiceResult<string>.Fail("Brand not found");

        if (request.CategoryId.HasValue && request.CategoryId != brand.CategoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
            if (category == null)
                return ServiceResult<string>.Fail("New category not found");
            brand.CategoryId = request.CategoryId.Value;
        }

        if (request.ImageId.HasValue && request.ImageId != brand.ImageId)
        {
            var image = await _imageRepository.GetByIdAsync(request.ImageId.Value);
            if (image == null)
                return ServiceResult<string>.Fail("New image not found");
            brand.ImageId = request.ImageId.Value;
        }

        if (!string.IsNullOrEmpty(request.Name) && request.Name != brand.Name)
        {
            var existingNames = await _brandRepository.FindAsync(b => b.Name == request.Name && b.CategoryId == brand.CategoryId && b.Id != brandId && b.Status != "deleted");
            if (existingNames.Any())
                return ServiceResult<string>.Fail("Brand with same name already exists in this category");

            brand.Name = request.Name;
            brand.SlugName = SlugHelper.GenerateSlug(request.Name);
        }

        if (request.Status != null)
        {
            brand.Status = request.Status;
        }

        brand.UpdateDate = DateTime.Now;
        brand.UpdateBy = userId;

        await _brandRepository.UpdateAsync(brand);
        await _brandRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("", "Brand updated successfully");
    }

    public async Task<ServiceResult<string>> DeleteBrand(long brandId, string userId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.Status == "deleted")
            return ServiceResult<string>.Fail("Brand not found");

        // Simple soft delete
        brand.Status = "deleted";
        brand.UpdateDate = DateTime.Now;
        brand.UpdateBy = userId;

        await _brandRepository.UpdateAsync(brand);
        await _brandRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("", "Brand deleted successfully");
    }

    public async Task<ServiceResult<PagedResult<BrandResponse>>> GetAllBrands(int pageIndex, int pageSize, string? status = "active")
    {
        var query = await _brandRepository.FindAsync(b => status == null || b.Status == status);
        
        var brands = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = query.Count();

        if (!brands.Any())
            return ServiceResult<PagedResult<BrandResponse>>.Fail("No brands found");

        var resultItems = brands.Select(b => new BrandResponse(b.Id, b.Name!, b.SlugName)).ToList();
        var pagedResult = new PagedResult<BrandResponse>(resultItems, totalCount, pageIndex, pageSize);

        return ServiceResult<PagedResult<BrandResponse>>.Success(pagedResult, "Brands retrieved successfully");
    }

    public async Task<ServiceResult<List<BrandResponse>>> GetBrandsByCategory(long categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.Status == "deleted")
            return ServiceResult<List<BrandResponse>>.Fail("Category not found");

        var brands = await _brandRepository.FindAsync(b => b.CategoryId == categoryId && b.Status != "deleted");
        if (!brands.Any())
            return ServiceResult<List<BrandResponse>>.Fail("No brands found in this category");

        var result = brands.Select(b => new BrandResponse(b.Id, b.Name, b.SlugName)).ToList();
        return ServiceResult<List<BrandResponse>>.Success(result, "Brands retrieved successfully");
    }

    public async Task<ServiceResult<BrandResponse>> GetBrandById(long brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.Status == "deleted")
            return ServiceResult<BrandResponse>.Fail("Brand not found");

        var result = new BrandResponse(brand.Id, brand.Name, brand.SlugName);
        return ServiceResult<BrandResponse>.Success(result, "Brand retrieved successfully");
    }
}
