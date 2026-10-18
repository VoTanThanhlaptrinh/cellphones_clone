using System.Security.Claims;
using cellPhoneS_backend.Constants;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/specifications")]
    [ApiController]
    [RequirePermission(Permissions.ProductRead)]
    public class AdminSpecificationController : BaseController
    {
        private readonly SpecificationService _specificationService;

        public AdminSpecificationController(SpecificationService specificationService)
        {
            _specificationService = specificationService;
        }

        // POST: api/admin/specifications
        // Creates a new specification category (e.g., "Display", "Camera") with optional initial details
        [HttpPost]
        [RequirePermission(Permissions.ProductWrite)]
        public async Task<ActionResult<ApiResponse<long>>> CreateSpecification([FromBody] CreateSpecificationRequest createSpecificationRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _specificationService.CreateSpecification(createSpecificationRequest, userId);
            return Ok(new ApiResponse<long>("Specification created successfully", result));
        }

        // POST: api/admin/specifications/{specificationId}/details
        // Adds a new detail to an existing specification
        [HttpPost("{specificationId}/details")]
        [RequirePermission(Permissions.ProductWrite)]
        public async Task<ActionResult<ApiResponse<long>>> AddSpecificationDetail(
            long specificationId, 
            [FromBody] CreateSpecificationDetailRequest createDetailRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _specificationService.AddSpecificationDetail(specificationId, createDetailRequest, userId);
            return Ok(new ApiResponse<long>("Specification detail added successfully", result));
        }

        // DELETE: api/admin/specifications/{specificationId}
        // Soft deletes a specification (sets status to "deleted")
        [HttpDelete("{specificationId}")]
        [RequirePermission(Permissions.ProductWrite)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSpecification(long specificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            await _specificationService.DeleteSpecification(specificationId, userId);
            return Ok(new ApiResponse<string>("Specification deleted successfully", ""));
        }
    }
}