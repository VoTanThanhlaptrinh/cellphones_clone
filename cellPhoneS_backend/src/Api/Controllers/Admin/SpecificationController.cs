using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/specifications")]
    [ApiController]
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
        public async Task<ActionResult<ApiResponse<long>>> CreateSpecification([FromBody] CreateSpecificationRequest createSpecificationRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _specificationService.CreateSpecification(createSpecificationRequest, userId);
            return HandleResult(result);
        }

        // POST: api/admin/specifications/{specificationId}/details
        // Adds a new detail to an existing specification
        [HttpPost("{specificationId}/details")]
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
            return HandleResult(result);
        }

        // DELETE: api/admin/specifications/{specificationId}
        // Soft deletes a specification (sets status to "deleted")
        [HttpDelete("{specificationId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSpecification(long specificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _specificationService.DeleteSpecification(specificationId, userId);
            return HandleResult(result);
        }
    }
}