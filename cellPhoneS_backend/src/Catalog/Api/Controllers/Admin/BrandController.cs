using System.Security.Claims;
using cellPhoneS_backend.Constants;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/brands")]
    [ApiController]
    [RequirePermission(Permissions.CatalogRead)]
    public class AdminBrandController : BaseController
    {
        private readonly BrandService _brandService;

        private readonly IAdminCatalogQueryService _queryService;

        public AdminBrandController(BrandService brandService, IAdminCatalogQueryService queryService)
        {
            _brandService = brandService;
            _queryService = queryService;
        }

        // GET: api/admin/brands?pageIndex=1&pageSize=20&search=&status=&categoryId=&sortBy=createDate&sortDir=desc
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminBrandView>>>> GetBrands([FromQuery] AdminCatalogFilter filter)
        {
            var result = await _queryService.GetBrands(filter);
            return Ok(new ApiResponse<PagedResult<AdminBrandView>>("Success", result));
        }

        // GET: api/admin/brands/{brandId}
        [HttpGet("{brandId:long}")]
        public async Task<ActionResult<ApiResponse<AdminBrandView>>> GetBrand(long brandId)
        {
            var result = await _queryService.GetBrand(brandId);
            return Ok(new ApiResponse<AdminBrandView>("Success", result));
        }

        // PATCH: api/admin/brands/{brandId}/restore   deleted -> active
        [HttpPatch("{brandId:long}/restore")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> RestoreBrand(long brandId)
        {
            await _brandService.RestoreBrand(brandId, CurrentUserId);
            return Ok(new ApiResponse<string>("Brand restored successfully", ""));
        }

        // POST: api/admin/brands
        // Creates a new brand under a category (e.g., "Apple" under "Smartphones")
        [HttpPost]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<long>>> CreateBrand([FromBody] CreateBrandRequest createBrandRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _brandService.CreateBrand(createBrandRequest, userId);
            return Ok(new ApiResponse<long>("Brand created successfully", result));
        }

        // PUT: api/admin/brands/{brandId}
        // Updates brand name, logo, or status
        [HttpPut("{brandId}")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateBrand(
            long brandId, 
            [FromBody] UpdateBrandRequest updateBrandRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            // Prevent ID tampering
            if (brandId != updateBrandRequest.Id)
            {
                return BadRequest(new ApiResponse<string>("Brand ID in URL does not match ID in request body", null!));
            }
            
            await _brandService.UpdateBrand(brandId, updateBrandRequest, userId);
            return Ok(new ApiResponse<string>("Brand updated successfully", ""));
        }

        // DELETE: api/admin/brands/{brandId}
        // Soft deletes a brand (sets status to "deleted")
        [HttpDelete("{brandId}")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteBrand(long brandId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            await _brandService.DeleteBrand(brandId, userId);
            return Ok(new ApiResponse<string>("Brand deleted successfully", ""));
        }
    }
}