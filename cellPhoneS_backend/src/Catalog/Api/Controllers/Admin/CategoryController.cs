using System.Security.Claims;
using cellPhoneS_backend.Constants;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/categories")]
    [ApiController]
    [RequirePermission(Permissions.CatalogRead)]
    public class AdminCategoryController : BaseController
    {
        private readonly CategoryService _categoryService;

        private readonly IAdminCatalogQueryService _queryService;

        public AdminCategoryController(CategoryService categoryService, IAdminCatalogQueryService queryService)
        {
            _categoryService = categoryService;
            _queryService = queryService;
        }

        // GET: api/admin/categories?pageIndex=1&pageSize=20&search=&status=&parentCategoryId=&sortBy=createDate&sortDir=desc
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminCategoryView>>>> GetCategories([FromQuery] AdminCatalogFilter filter)
        {
            var result = await _queryService.GetCategories(filter);
            return Ok(new ApiResponse<PagedResult<AdminCategoryView>>("Success", result));
        }

        // GET: api/admin/categories/{categoryId}
        [HttpGet("{categoryId:long}")]
        public async Task<ActionResult<ApiResponse<AdminCategoryView>>> GetCategory(long categoryId)
        {
            var result = await _queryService.GetCategory(categoryId);
            return Ok(new ApiResponse<AdminCategoryView>("Success", result));
        }

        // PATCH: api/admin/categories/{categoryId}/restore   deleted -> active
        [HttpPatch("{categoryId:long}/restore")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> RestoreCategory(long categoryId)
        {
            await _categoryService.RestoreCategory(categoryId, CurrentUserId);
            return Ok(new ApiResponse<string>("Category restored successfully", ""));
        }

        // POST: api/admin/categories
        // Creates a new category
        [HttpPost]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<long>>> CreateCategory([FromBody] CreateCategoryRequest createCategoryRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _categoryService.CreateCategory(createCategoryRequest, userId);
            return Ok(new ApiResponse<long>("Category created successfully", result));
        }

        // PUT: api/admin/categories/{categoryId}
        // Updates category details
        [HttpPut("{categoryId}")]
        [RequirePermission(Permissions.CatalogWrite)]
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
            
            await _categoryService.UpdateCategory(categoryId, updateCategoryRequest, userId);
            return Ok(new ApiResponse<string>("Category updated successfully", ""));
        }

        // DELETE: api/admin/categories/{categoryId}
        // Soft deletes a category
        [HttpDelete("{categoryId}")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(long categoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            await _categoryService.DeleteCategory(categoryId, userId);
            return Ok(new ApiResponse<string>("Category deleted successfully", ""));
        }
    }
}
