using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/brands")]
    [ApiController]
    public class AdminBrandController : BaseController
    {
        private readonly BrandService _brandService;

        public AdminBrandController(BrandService brandService)
        {
            _brandService = brandService;
        }

        // POST: api/admin/brands
        // Creates a new brand under a category (e.g., "Apple" under "Smartphones")
        [HttpPost]
        public async Task<ActionResult<ApiResponse<long>>> CreateBrand([FromBody] CreateBrandRequest createBrandRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _brandService.CreateBrand(createBrandRequest, userId);
            return HandleResult(result);
        }

        // PUT: api/admin/brands/{brandId}
        // Updates brand name, logo, or status
        [HttpPut("{brandId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateBrand(
            long brandId, 
            [FromBody] CreateBrandRequest updateBrandRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _brandService.UpdateBrand(brandId, updateBrandRequest, userId);
            return HandleResult(result);
        }

        // DELETE: api/admin/brands/{brandId}
        // Soft deletes a brand (sets status to "deleted")
        [HttpDelete("{brandId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteBrand(long brandId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _brandService.DeleteBrand(brandId, userId);
            return HandleResult(result);
        }
    }
}