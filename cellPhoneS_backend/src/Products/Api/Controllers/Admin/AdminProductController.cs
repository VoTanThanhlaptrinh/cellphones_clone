using cellPhoneS_backend.Constants;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.J2O;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>
    /// Sản phẩm cho trang quản trị: đọc (list/detail), ghi, đổi trạng thái, xoá mềm và khôi phục.
    /// Cổng `admin.access` do RouteConfig chặn; từng action khai báo permission bằng [RequirePermission].
    /// Mọi thao tác ghi làm mới chỉ mục tìm kiếm của storefront (ProductSearchService).
    /// </summary>
    [Route("api/admin/products")]
    [ApiController]
    [RequirePermission(Permissions.ProductRead)]
    public class AdminProductController : BaseController
    {
        private readonly ProductService _productService;
        private readonly ProductSearchService _productSearchService;
        private readonly IAdminProductQueryService _queryService;

        public AdminProductController(ProductService productService, ProductSearchService productSearchService,
            IAdminProductQueryService queryService)
        {
            _productService = productService;
            _productSearchService = productSearchService;
            _queryService = queryService;
        }

        // GET: api/admin/products?pageIndex=1&pageSize=20&search=&status=&categoryId=&brandId=&lowStock=5&sortBy=createDate&sortDir=desc
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminProductView>>>> GetProducts([FromQuery] AdminProductFilter filter)
        {
            var result = await _queryService.GetProducts(filter);
            return Ok(new ApiResponse<PagedResult<AdminProductView>>("Success", result));
        }

        // GET: api/admin/products/{productId}
        [HttpGet("{productId:long}")]
        public async Task<ActionResult<ApiResponse<AdminProductDetail>>> GetProduct(long productId)
        {
            var result = await _queryService.GetProduct(productId);
            return Ok(new ApiResponse<AdminProductDetail>("Success", result));
        }

        // POST: api/admin/products
        // Creates a new product with all its dependencies (images, specifications, commitments, colors, inventory)
        [HttpPost]
        [RequirePermission(Permissions.ProductWrite)]
        public async Task<ActionResult<ApiResponse<string>>> CreateProduct([FromBody] AddProductRequest addProductRequest)
        {
            var result = await _productService.CreateProduct(addProductRequest, CurrentUserId);
            await _productSearchService.RefreshAsync();
            return Ok(new ApiResponse<string>("Product created successfully", result));
        }

        // PUT: api/admin/products/{productId}
        [HttpPut("{productId:long}")]
        [RequirePermission(Permissions.ProductWrite)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateProduct(long productId, [FromBody] UpdateProductRequest updateProductRequest)
        {
            await _productService.UpdateProduct(productId, updateProductRequest, CurrentUserId);
            await _productSearchService.RefreshAsync();
            return Ok(new ApiResponse<string>("Product updated successfully", ""));
        }

        // PATCH: api/admin/products/{productId}/status   body { "status": "active" | "inactive" }
        [HttpPatch("{productId:long}/status")]
        [RequirePermission(Permissions.ProductWrite)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(long productId, [FromBody] UpdateProductStatusRequest request)
        {
            await _productService.SetProductStatus(productId, request.Status, CurrentUserId);
            await _productSearchService.RefreshAsync();
            return Ok(new ApiResponse<string>("Product status updated successfully", ""));
        }

        // DELETE: api/admin/products/{productId}
        // Soft deletes a product
        [HttpDelete("{productId:long}")]
        [RequirePermission(Permissions.ProductDelete)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteProduct(long productId)
        {
            await _productService.DeleteProduct(productId, CurrentUserId);
            await _productSearchService.RefreshAsync();
            return Ok(new ApiResponse<string>("Product deleted successfully", ""));
        }

        // PATCH: api/admin/products/{productId}/restore   deleted -> active
        [HttpPatch("{productId:long}/restore")]
        [RequirePermission(Permissions.ProductDelete)]
        public async Task<ActionResult<ApiResponse<string>>> RestoreProduct(long productId)
        {
            await _productService.RestoreProduct(productId, CurrentUserId);
            await _productSearchService.RefreshAsync();
            return Ok(new ApiResponse<string>("Product restored successfully", ""));
        }
    }
}
