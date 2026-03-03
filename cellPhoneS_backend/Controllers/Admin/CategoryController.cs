using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/categories")]
    [ApiController]
    public class AdminCategoryController : BaseController
    {
        private readonly CategoryService _categoryService;

        public AdminCategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // POST: api/admin/categories
        // Creates a new category
        [HttpPost]
        public async Task<ActionResult<ApiResponse<long>>> CreateCategory([FromBody] CreateCategoryRequest createCategoryRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _categoryService.CreateCategory(createCategoryRequest, userId);
            return HandleResult(result);
        }

        // PUT: api/admin/categories/{categoryId}
        // Updates category details
        [HttpPut("{categoryId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateCategory(
            long categoryId, 
            [FromBody] UpdateCategoryRequest updateCategoryRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            // Prevent ID tampering
            if (categoryId != updateCategoryRequest.Id)
            {
                return BadRequest(new ApiResponse<string>("Category ID in URL does not match ID in request body", null!));
            }
            
            var result = await _categoryService.UpdateCategory(categoryId, updateCategoryRequest, userId);
            return HandleResult(result);
        }

        // DELETE: api/admin/categories/{categoryId}
        // Soft deletes a category
        [HttpDelete("{categoryId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(long categoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _categoryService.DeleteCategory(categoryId, userId);
            return HandleResult(result);
        }
    }
}
