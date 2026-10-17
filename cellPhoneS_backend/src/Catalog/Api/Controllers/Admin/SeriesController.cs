using System.Security.Claims;
using cellPhoneS_backend.Constants;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/series")]
    [ApiController]
    [RequirePermission(Permissions.CatalogRead)]
    public class AdminSeriesController : BaseController
    {
        private readonly SeriesService _seriesService;

        public AdminSeriesController(SeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        // POST: api/admin/series
        // Creates a new product series under a brand (e.g., "iPhone 15 Series" under "Apple")
        [HttpPost]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<long>>> CreateSeries([FromBody] CreateSeriesRequest createSeriesRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _seriesService.CreateSeries(createSeriesRequest, userId);
            return Ok(new ApiResponse<long>("Series created successfully", result));
        }

        // PUT: api/admin/series/{seriesId}
        // Updates series name or status
        [HttpPut("{seriesId}")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateSeries(
            long seriesId, 
            [FromBody] UpdateSeriesRequest updateSeriesRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            // Prevent ID tampering
            if (seriesId != updateSeriesRequest.Id)
            {
                return BadRequest(new ApiResponse<string>("Series ID in URL does not match ID in request body", null!));
            }
            
            await _seriesService.UpdateSeries(seriesId, updateSeriesRequest, userId);
            return Ok(new ApiResponse<string>("Series updated successfully", ""));
        }

        // DELETE: api/admin/series/{seriesId}
        // Soft deletes a series (sets status to "deleted")
        [HttpDelete("{seriesId}")]
        [RequirePermission(Permissions.CatalogWrite)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSeries(long seriesId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            await _seriesService.DeleteSeries(seriesId, userId);
            return Ok(new ApiResponse<string>("Series deleted successfully", ""));
        }
    }
}