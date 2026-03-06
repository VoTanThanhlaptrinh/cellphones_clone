using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.User
{
    [Route("api/brands")]
    [ApiController]
    public class BrandController : BaseController
    {
        private readonly BrandService _brandService;

        public BrandController(BrandService brandService)
        {
            _brandService = brandService;
        }

        // GET: api/brands/category/{categoryId}
        // Retrieves all brands for a specific category for public viewing
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<BrandResponse>>>> GetBrandsByCategory(long categoryId)
        {
            var result = await _brandService.GetBrandsByCategory(categoryId);
            return HandleResult(result);
        }

        // GET: api/brands/{brandId}
        // Retrieves brand details by ID including logo and series for public viewing
        [HttpGet("{brandId}")]
        public async Task<ActionResult<ApiResponse<BrandResponse>>> GetBrandById(long brandId)
        {
            var result = await _brandService.GetBrandById(brandId);
            return HandleResult(result);
        }

        // GET: api/brands
        // Retrieves an active paginated list of all brands
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<BrandResponse>>>> GetAllBrands(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = "active")
        {
            var result = await _brandService.GetAllBrands(pageIndex, pageSize, status);
            return HandleResult(result);
        }
    }
}