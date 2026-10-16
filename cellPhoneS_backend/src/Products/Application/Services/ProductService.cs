using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;

namespace cellphones_backend.Services;

public interface ProductService
{
    Task<List<ProductView>> GetProducts(int page, int pageSize);
    Task<ProductViewDetail> GetProductDetails(long id);
    Task UpdateProduct(long id, UpdateProductRequest request, string userId);
    Task DeleteProduct(long id, string userId);
    Task<string> CreateProduct(AddProductRequest productRequest, string userId);
    Task<List<ProductView>> GetProductsByCategory(long categoryId, int page, int pageSize);
    Task<List<ProductView>> GetProductsBySeries(long seriesId, int page, int pageSize);
    Task<List<ProductView>> GetProductsByBrand(long brandId, int page, int pageSize);
    Task<List<ProductView>> GetProductsForInfinityScroll(string tag, string slugName, long? countCursor);
}