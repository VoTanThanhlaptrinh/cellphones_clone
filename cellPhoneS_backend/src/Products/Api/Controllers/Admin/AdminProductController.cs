using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.J2O;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>
    /// Ghi sản phẩm cho trang quản trị. Danh sách (GET api/admin/products) nằm ở AdminQueryController.
    /// Quyền ADMIN được kiểm tra bởi `/api/admin/*` trong RouteConfig.
    /// </summary>
    [Route("api/admin/products")]
    [ApiController]
    public class AdminProductController : BaseController
    {
        private readonly ProductService _productService;
        private readonly ProductSearchService _productSearchService;

        public AdminProductController(ProductService productService, ProductSearchService productSearchService)
        {
            _productService = productService;
            _productSearchService = productSearchService;
        }

        // POST: api/admin/products
        // Creates a new product with all its dependencies (images, specifications, commitments, colors, inventory)
        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> CreateProduct([FromBody] AddProductRequest addProductRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }

            var result = await _productService.CreateProduct(addProductRequest, userId);
            _productSearchService.RefreshCache();
            return Ok(new ApiResponse<string>("Product created successfully", result));
        }

        // PUT: api/admin/products/{productId}
        [HttpPut("{productId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateProduct(long productId, [FromBody] UpdateProductRequest updateProductRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }

            await _productService.UpdateProduct(productId, updateProductRequest, userId);
            _productSearchService.RefreshCache();
            return Ok(new ApiResponse<string>("Product updated successfully", ""));
        }

        // DELETE: api/admin/products/{productId}
        // Soft deletes a product
        [HttpDelete("{productId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteProduct(long productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }

            await _productService.DeleteProduct(productId, userId);
            _productSearchService.RefreshCache();
            return Ok(new ApiResponse<string>("Product deleted successfully", ""));
        }
    }
}
