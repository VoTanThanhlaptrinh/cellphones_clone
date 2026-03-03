using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers.User
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : BaseController
    {
        private readonly CategoryService _categoryService;
        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        
        [HttpGet("{slugName}")]
        public async Task<ActionResult<ApiResponse<CategoryDetailResponse>>> GetCategories(string slugName)
        {
            var res = await _categoryService.GetCategoryDetailBySlug(slugName);
            return HandleResult(res);
        }
        
        [HttpGet("loadMore/{id}/{page}")]
        public async Task<ActionResult<ApiResponse<List<ProductView>>>> LoadMoreProduct(long id, int page)
        {
            var res = await _categoryService.GetProductOfCategory(id, page);
            return HandleResult(res);
        }

        // GET: api/category
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<CategoryResponse>>>> GetAllCategories(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = "active")
        {
            var res = await _categoryService.GetAllCategories(pageIndex, pageSize, status);
            return HandleResult(res);
        }

        // GET: api/category/id/{categoryId}
        [HttpGet("id/{categoryId}")]
        public async Task<ActionResult<ApiResponse<CategoryDetailResponse>>> GetCategoryById(long categoryId)
        {
            var res = await _categoryService.GetCategoryDetail(categoryId);
            return HandleResult(res);
        }
    }
}
