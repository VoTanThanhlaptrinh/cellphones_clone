using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class ProductServiceImpl : ProductService
{
    private readonly IProductRepository _productDBcontext;
    private readonly ICategoryProductRepository _categoryProductDBcontext;
    public ProductServiceImpl(IProductRepository productDBcontext, ICategoryProductRepository categoryProductDBcontext)
    {
        _productDBcontext = productDBcontext;
        _categoryProductDBcontext = categoryProductDBcontext;
    }

    public async Task<ServiceResult<string>> CreateProduct(AddProductRequest productRequest, string userId)
    {
        // TODO: Implement business logic using the productRequest and userId here
        // Step-by-step implementation guide:
        // 1. Begin a database transaction
        // using var transaction = await _context.Database.BeginTransactionAsync();
        // try {
        //     2. Upload thumbnail image to Azure Blob Storage and save Image entity
        //     var thumbnailImage = await _imageService.UploadImageAsync(productRequest.ThumbnailImage);
        
        //     3. Upload product images to Azure Blob Storage and save Image entities
        //     var productImages = new List<Image>();
        //     if (productRequest.ProductImages != null)
        //     {
        //         foreach (var imgDto in productRequest.ProductImages)
        //         {
        //             var image = await _imageService.UploadImageAsync(imgDto);
        //             productImages.Add(image);
        //         }
        //     }
        
        //     4. Create the main Product entity
        //     var product = new Product
        //     {
        //         Name = productRequest.Name,
        //         BasePrice = productRequest.BasePrice,
        //         SalePrice = productRequest.SalePrice,
        //         Version = productRequest.Version,
        //         Status = productRequest.Status ?? "active",
        //         ImageUrl = thumbnailImage.BlobUrl,
        //         CreateDate = DateTime.UtcNow,
        //         UpdateDate = DateTime.UtcNow,
        //         CreateBy = userId,
        //         UpdateBy = userId
        //     };
        //     await _productRepository.AddAsync(product);
        //     await _productRepository.SaveChangesAsync();
        
        //     5. Create CategoryProduct relationship
        //     var categoryProduct = new CategoryProduct
        //     {
        //         CategoryId = productRequest.CategoryId.Value,
        //         ProductId = product.Id,
        //         Status = "active",
        //         CreateDate = DateTime.UtcNow,
        //         UpdateDate = DateTime.UtcNow,
        //         CreateBy = userId,
        //         UpdateBy = userId
        //     };
        //     await _categoryProductRepository.AddAsync(categoryProduct);
        
        //     6. Create ProductImage relationships
        //     foreach (var image in productImages)
        //     {
        //         var productImage = new ProductImage
        //         {
        //             ProductId = product.Id,
        //             ImageId = image.Id,
        //             Status = "active",
        //             CreateDate = DateTime.UtcNow,
        //             UpdateDate = DateTime.UtcNow,
        //             CreateBy = userId,
        //             UpdateBy = userId
        //         };
        //         await _productImageRepository.AddAsync(productImage);
        //     }
        
        //     7. Create Commitment entities
        //     if (productRequest.Commitments != null)
        //     {
        //         foreach (var commitmentText in productRequest.Commitments)
        //         {
        //             var commitment = new Commitment
        //             {
        //                 ProductCommitmentId = product.Id,
        //                 Context = commitmentText,
        //                 Status = "active",
        //                 CreateDate = DateTime.UtcNow,
        //                 UpdateDate = DateTime.UtcNow,
        //                 CreateBy = userId,
        //                 UpdateBy = userId
        //             };
        //             await _commitmentRepository.AddAsync(commitment);
        //         }
        //     }
        
        //     8. Create/link Color entities
        //     var colorIds = new List<long>();
        //     if (productRequest.Colors != null)
        //     {
        //         foreach (var colorDto in productRequest.Colors)
        //         {
        //             Color color;
        //             if (colorDto.Id.HasValue)
        //             {
        //                 // Use existing color
        //                 color = await _colorRepository.GetByIdAsync(colorDto.Id.Value);
        //                 if (color == null)
        //                 {
        //                     throw new Exception($"Color with ID {colorDto.Id.Value} not found");
        //                 }
        //             }
        //             else
        //             {
        //                 // Create new color
        //                 color = new Color
        //                 {
        //                     Name = colorDto.Name,
        //                     ColorCode = colorDto.ColorCode,
        //                     Status = "active",
        //                     CreateDate = DateTime.UtcNow,
        //                     UpdateDate = DateTime.UtcNow,
        //                     CreateBy = userId,
        //                     UpdateBy = userId
        //                 };
        //                 await _colorRepository.AddAsync(color);
        //                 await _colorRepository.SaveChangesAsync();
        //             }
        //             colorIds.Add(color.Id);
        //         }
        //     }
        
        //     9. Create Specification and SpecificationDetail entities
        //     if (productRequest.Specifications != null)
        //     {
        //         foreach (var specDto in productRequest.Specifications)
        //         {
        //             // Check if specification already exists
        //             var specification = await _specificationRepository
        //                 .FindByCondition(s => s.Name == specDto.Name)
        //                 .FirstOrDefaultAsync();
        //             
        //             if (specification == null)
        //             {
        //                 specification = new Specification
        //                 {
        //                     Name = specDto.Name,
        //                     Status = "active",
        //                     CreateDate = DateTime.UtcNow,
        //                     UpdateDate = DateTime.UtcNow,
        //                     CreateBy = userId,
        //                     UpdateBy = userId
        //                 };
        //                 await _specificationRepository.AddAsync(specification);
        //                 await _specificationRepository.SaveChangesAsync();
        //             }
        //             
        //             // Create ProductSpecification relationship
        //             var productSpecification = new ProductSpecification
        //             {
        //                 ProductId = product.Id,
        //                 SpecificationId = specification.Id,
        //                 Status = "active",
        //                 CreateDate = DateTime.UtcNow,
        //                 UpdateDate = DateTime.UtcNow,
        //                 CreateBy = userId,
        //                 UpdateBy = userId
        //             };
        //             await _productSpecificationRepository.AddAsync(productSpecification);
        //             
        //             // Create SpecificationDetails
        //             if (specDto.SpecDetails != null)
        //             {
        //                 foreach (var detailDto in specDto.SpecDetails)
        //                 {
        //                     var specificationDetail = new SpecificationDetail
        //                     {
        //                         SpecificationId = specification.Id,
        //                         Name = detailDto.Name,
        //                         Value = detailDto.Value,
        //                         Status = "active",
        //                         CreateDate = DateTime.UtcNow,
        //                         UpdateDate = DateTime.UtcNow,
        //                         CreateBy = userId,
        //                         UpdateBy = userId
        //                     };
        //                     await _specificationDetailRepository.AddAsync(specificationDetail);
        //                 }
        //             }
        //         }
        //     }
        
        //     10. Create ProductSpecification relationships
        //     11. Create initial Store (inventory) entries
        //     if (productRequest.InitialInventory != null)
        //     {
        //         foreach (var storeDto in productRequest.InitialInventory)
        //         {
        //             var store = new Store
        //             {
        //                 StoreHouseId = storeDto.StoreHouseId,
        //                 ProductId = product.Id,
        //                 ColorId = storeDto.ColorId,
        //                 Quantity = storeDto.Quantity,
        //                 Status = "active",
        //                 CreateDate = DateTime.UtcNow,
        //                 UpdateDate = DateTime.UtcNow,
        //                 CreateBy = userId,
        //                 UpdateBy = userId
        //             };
        //             await _storeRepository.AddAsync(store);
        //         }
        //     }
        
        //     12. Save all changes and commit transaction
        //     await _context.SaveChangesAsync();
        //     await transaction.CommitAsync();
        //     
        //     return ServiceResult<string>.Success("Product created successfully", product.Id.ToString());
        // }
        // catch (Exception ex)
        // {
        //     await transaction.RollbackAsync();
        //     return ServiceResult<string>.Fail($"Failed to create product: {ex.Message}", ServiceErrorType.ServerError);
        // }
        
        // Placeholder return until implementation is complete
        return ServiceResult<string>.Success("Product creation endpoint is ready but not yet implemented", null);
    }

    public async Task<ServiceResult<string>> DeleteProduct(long id, string userId)
    {
        var product = await _productDBcontext.GetByIdAsync(id);
        if (product == null)
        {
            return ServiceResult<string>.Fail("Product not found", ServiceErrorType.NotFound);
        }
        product.Status = "deleted";
        product.UpdateDate = DateTime.UtcNow;
        product.UpdateBy = userId;
        await _productDBcontext.UpdateAsync(product);
        return ServiceResult<string>.Success("Product deleted successfully", null!);
    }

    public async Task<ServiceResult<ProductViewDetail>> GetProductDetails(long id)
    {
        var product = await _productDBcontext.GetProductDetails(id);
        if (product == null)
        {
            return ServiceResult<ProductViewDetail>.Fail("Product not found", ServiceErrorType.NotFound);
        }
        return ServiceResult<ProductViewDetail>.Success(product,"success");
    }

    public async Task<ServiceResult<List<ProductView>>> GetProducts(int page, int pageSize)
    {
        var productViews = await _productDBcontext.GetAllAsync(page, pageSize);
        if (productViews == null || !productViews.Any())
        {
            return ServiceResult<List<ProductView>>.Fail("List product not found", ServiceErrorType.NotFound);
        }
        return ServiceResult<List<ProductView>>.Success(productViews.ToList(), "success");
    }

    public async Task<ServiceResult<List<ProductView>>> GetProductsByBrand(long brandId, int page, int pageSize)
    {
        var productViews = await _categoryProductDBcontext.GetProductsByBrandId(brandId, page, pageSize);
        if (productViews == null || !productViews.Any())
        {
            return ServiceResult<List<ProductView>>.Fail("No products found", ServiceErrorType.NotFound);
        }
        return ServiceResult<List<ProductView>>.Success(productViews.ToList(), "success");
    }

    public async Task<ServiceResult<List<ProductView>>> GetProductsByCategory(long categoryId, int page, int pageSize)
    {
        var productViews = await _categoryProductDBcontext.GetProductsByCategoryId(categoryId, page, pageSize);
        if (productViews == null || !productViews.Any())
        {
            return ServiceResult<List<ProductView>>.Fail("No products found", ServiceErrorType.NotFound);
        }
        return ServiceResult<List<ProductView>>.Success(productViews.ToList(), "success");
    }

    public async Task<ServiceResult<List<ProductView>>> GetProductsBySeries(long seriesId, int page, int pageSize)
    {
        var productViews = await _categoryProductDBcontext.GetProductsBySeriesId(seriesId, page, pageSize);
        if (productViews == null || !productViews.Any())
        {
            return ServiceResult<List<ProductView>>.Fail("No products found", ServiceErrorType.NotFound);
        }
        return ServiceResult<List<ProductView>>.Success(productViews.ToList(), "success");
    }

    public Task<ServiceResult<List<ProductView>>> SearchProducts(string keyword, int page, int pageSize)
    {
        return null!;
    }

    public async Task<ServiceResult<string>> UpdateProduct(long id, UpdateProductRequest request, string userId)
    {
        // TODO: Implement business logic using the id, request, and userId here
        // 1. Find product by ID
        // 2. If not found, return ServiceResult<string>.Fail with NotFound error
        // 3. Update fields from request
        // 4. Update UpdateDate = DateTime.UtcNow
        // 5. Update UpdateBy = userId
        // 6. Save changes
        // 7. Return ServiceResult<string>.Success
        
        throw new NotImplementedException();
    }

}
