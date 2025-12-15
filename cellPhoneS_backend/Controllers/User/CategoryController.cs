using cellphones_backend.DTOs.Responses;
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
        [HttpGet("{id}/{page}")]
        public async Task<ActionResult<ApiResponse<CategoryDetailView>>> GetCategories(long id, int page)
        {
            var res = await _categoryService.GetCategoryDetail(id, page);
            return HandleResult(res);
        }
        [HttpGet("loadMore/{id}/{page}")]
        public async Task<ActionResult<ApiResponse<List<ProductView>>>> LoadMoreProduct(long id, int page)
        {
            var res = await _categoryService.GetProductOfCategory(id, page);
            return HandleResult(res);
        }
    }
}
