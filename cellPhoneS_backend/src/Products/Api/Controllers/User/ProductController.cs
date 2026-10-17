using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : BaseController
    {
        private readonly ProductService _productService;
        
        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("list/{page}/{size}")]
        public async Task<ActionResult<ApiResponse<List<ProductView>>>> ListProduct(int page, int size)
        {
            var res = await _productService.GetProducts(page, size);
            return Ok(new ApiResponse<List<ProductView>>("success", res));
        }

        [HttpGet("brand/{brandId}/{page}/{size}")]
        public async Task<ActionResult<ApiResponse<List<ProductView>>>> ListProductByBrand(long brandId, int page, int size)
        {
            var res = await _productService.GetProductsByBrand(brandId, page, size);
            return Ok(new ApiResponse<List<ProductView>>("success", res));
        }

        [HttpGet("series/{seriesId}/{page}/{size}")]
        public async Task<ActionResult<ApiResponse<List<ProductView>>>> ListProductBySeries(long seriesId, int page, int size)
        {
            var res = await _productService.GetProductsBySeries(seriesId, page, size);
            return Ok(new ApiResponse<List<ProductView>>("success", res));
        }

        [HttpGet("category/{categoryId}/{page}/{size}")]
        public async Task<ActionResult<ApiResponse<List<ProductView>>>> ListProductByCategory(long categoryId, int page, int size)
        {
            var res = await _productService.GetProductsByCategory(categoryId, page, size);
            return Ok(new ApiResponse<List<ProductView>>("success", res));
        }
        [HttpGet("{tag}/{slugName}")]
        public async Task<ActionResult<ApiResponse<List<ProductView>>>> GetProductsForInfinityScroll(string tag, string slugName, [FromQuery] long? cursor = null)
        {
                var res = await _productService.GetProductsForInfinityScroll(tag, slugName, cursor);
                return Ok(new ApiResponse<List<ProductView>>("success", res));
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<ApiResponse<ProductViewDetail>>> GetProduct(long productId)
        {
            var res = await _productService.GetProductDetails(productId);
            return Ok(new ApiResponse<ProductViewDetail>("success", res));
        }
    }
}
