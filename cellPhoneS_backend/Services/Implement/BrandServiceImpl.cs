using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
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
        // TODO: Implement business logic using the request and userId here
        // 1. Verify category exists using _categoryRepository.GetByIdAsync()
        // 2. If ImageId provided, verify image exists using _imageRepository.GetByIdAsync()
        // 3. Check if brand with same name already exists in this category
        // 4. Create Brand entity
        // 5. Set CategoryId, Name, ImageId, Status from request
        // 6. Set CreateDate = UpdateDate = DateTime.UtcNow
        // 7. Set CreateBy = UpdateBy = userId
        // 7. Save to database using _brandRepository.AddAsync()
        // 8. Return ServiceResult<long>.Success with new Brand ID
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<string>> UpdateBrand(long brandId, CreateBrandRequest request, string userId)
    {
        // TODO: Implement business logic using the request and userId here
        // 1. Find brand by ID using _brandRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<string>.Fail with NotFound error
        // 3. If CategoryId changed, verify new category exists
        // 4. If ImageId changed, verify new image exists
        // 5. Update Name, CategoryId, ImageId, Status if provided
        // 6. Update UpdateDate = DateTime.UtcNow
        // 7. Update UpdateBy = userId
        // 5. Save changes using _brandRepository.UpdateAsync()
        // 6. Return ServiceResult<string>.Success
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<string>> DeleteBrand(long brandId, string userId)
    {
        // TODO: Implement business logic using the userId here
        // 1. Find brand by ID using _brandRepository.GetByIdAsync()
        // 2. If not found, return ServiceResult<string>.Fail with NotFound error
        // 3. Check if brand has series or products (consider navigation properties)
        // 4. If has dependencies, return error or handle cascade soft delete
        // 5. Set Status = "deleted"
        // 6. Update UpdateDate = DateTime.UtcNow
        // 7. Update UpdateBy = userId
        // 7. Save changes using _brandRepository.UpdateAsync()
        // 8. Return ServiceResult<string>.Success
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<List<BrandView>>> GetAllBrands()
    {
        // TODO: Implement business logic here
        // 1. Query all brands where Status != "deleted" using _brandRepository
        // 2. Project to BrandView DTOs
        // 3. If no brands found, return ServiceResult<List<BrandView>>.Fail with NotFound error
        // 4. Return ServiceResult<List<BrandView>>.Success with brand list
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<List<BrandView>>> GetBrandsByCategory(long categoryId)
    {
        // TODO: Implement business logic here
        // 1. Verify category exists using _categoryRepository
        // 2. Query brands for this category where Status != "deleted"
        // 3. Project to BrandView DTOs
        // 4. If no brands found, return ServiceResult<List<BrandView>>.Fail with NotFound error
        // 5. Return ServiceResult<List<BrandView>>.Success with brand list
        
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<BrandView>> GetBrandById(long brandId)
    {
        // TODO: Implement business logic here
        // 1. Find brand by ID with related data (Image, Category) using _brandRepository
        // 2. If not found or Status = "deleted", return ServiceResult<BrandView>.Fail with NotFound error
        // 3. Project to BrandView DTO
        // 4. Return ServiceResult<BrandView>.Success
        
        throw new NotImplementedException();
    }
}
