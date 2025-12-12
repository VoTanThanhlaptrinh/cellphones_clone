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
    public Task<ServiceResult<string>> CreateProduct(AddProductRequest productRequest)
    {
        return null!;
    }

    public async Task<ServiceResult<string>> DeleteProduct(long id)
    {
        var product = await _productDBcontext.GetByIdAsync(id);
        if (product == null)
        {
            return ServiceResult<string>.Fail("Product not found", ServiceErrorType.NotFound);
        }
        product.Status = "deleted";
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

    public async Task<ServiceResult<string>> UpdateProduct(long id)
    {
        return null!;
    }

}
