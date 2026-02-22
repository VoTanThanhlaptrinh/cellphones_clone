
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface ProductService
{
    Task<ServiceResult<List<ProductView>>> GetProducts(int page, int pageSize);
    Task<ServiceResult<ProductViewDetail>> GetProductDetails(long id);
    Task<ServiceResult<string>> UpdateProduct(long id, UpdateProductRequest request, string userId);
    Task<ServiceResult<string>> DeleteProduct(long id, string userId);
    Task<ServiceResult<string>> CreateProduct(AddProductRequest productRequest, string userId);
    Task<ServiceResult<List<ProductView>>> GetProductsByCategory(long categoryId, int page, int pageSize);
    Task<ServiceResult<List<ProductView>>> GetProductsBySeries(long seriesId, int page, int pageSize);
    Task<ServiceResult<List<ProductView>>> GetProductsByBrand(long brandId, int page, int pageSize);
}