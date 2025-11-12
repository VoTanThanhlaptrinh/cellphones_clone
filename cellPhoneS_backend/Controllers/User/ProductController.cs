using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Services;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        public ProductController(ProductService productService)
        {
            _productService = productService;
        }
        [HttpGet("list/{page}/{size}")]
        public Task<ActionResult<ApiResponse<List<ProductView>>>> ListProduct(int page, int size)
        {
            return null!;
        }
        [HttpGet("{productId}")]
        public Task<ActionResult<ApiResponse<ProductView>>> GetProduct(long productId)
        {
            return null!;
        }
        [HttpPost()]
        public Task<ActionResult<ApiResponse<string>>> AddProduct([FromBody] AddProductRequest addProductRequest)
        {
            return null!;
        }
        [HttpPut()]
        public Task<ActionResult<ApiResponse<string>>> UpdateProduct([FromBody] UpdateProductRequest updateProductRequest)
        {
            return null!;
        }
        [HttpDelete("{productId}")]
        public Task<ActionResult<ApiResponse<string>>> DeleteProduct(long productId)
        {
            return null!;
        }
    }

}
