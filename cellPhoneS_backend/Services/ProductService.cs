using System;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;

namespace cellphones_backend.Services;

public interface ProductService
{
    public ApiResponse<List<ProductView>> GetProducts(int page, int pageSize);
    public ApiResponse<ProductViewDetail> GetDetails(long id);
    public ApiResponse<string> UpdateProduct(long id);
    public ApiResponse<string> DeleteProduct(long id);
    public ApiResponse<string> CreateProduct(AddProductRequest productRequest);
}