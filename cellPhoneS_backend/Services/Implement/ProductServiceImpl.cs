using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;

namespace cellphones_backend.Services.Implement;
public class ProductServiceImpl : ProductService
{
    private readonly ApplicationDbContext _context;
    public ProductServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }
    public ApiResponse<string> CreateProduct(AddProductRequest productRequest)
    {
        return null!;
    }

    public ApiResponse<string> DeleteProduct(long id)
    {
        return null!;
    }

    public async Task<ApiResponse<ProductViewDetail>> GetDetails(long id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return new ApiResponse<ProductViewDetail>("Product not found", null!, 404);
        }

        var productViewDetail = new ProductViewDetail
        (
            product.Id,
            product.Name,
            product.BasePrice,
            product.SalePrice,
            product.ImageUrl,
            product.CreateDate,
            product.UpdateDate,
            product.CreateUser?.Email,
            product.UpdateUser?.Email
        );

        return new ApiResponse<ProductViewDetail>("success", productViewDetail, 200);
    }

    public ApiResponse<List<ProductView>> GetProducts(int page, int pageSize)
    {
        
        return null!;
    }

    public ApiResponse<string> UpdateProduct(long id)
    {
        return null!;
    }
}
