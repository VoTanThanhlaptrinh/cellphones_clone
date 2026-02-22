using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/series")]
    [ApiController]
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
        public async Task<ActionResult<ApiResponse<long>>> CreateSeries([FromBody] CreateSeriesRequest createSeriesRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _seriesService.CreateSeries(createSeriesRequest, userId);
            return HandleResult(result);
        }

        // PUT: api/admin/series/{seriesId}
        // Updates series name or status
        [HttpPut("{seriesId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateSeries(
            long seriesId, 
            [FromBody] CreateSeriesRequest updateSeriesRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _seriesService.UpdateSeries(seriesId, updateSeriesRequest, userId);
            return HandleResult(result);
        }

        // DELETE: api/admin/series/{seriesId}
        // Soft deletes a series (sets status to "deleted")
        [HttpDelete("{seriesId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSeries(long seriesId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _seriesService.DeleteSeries(seriesId, userId);
            return HandleResult(result);
        }
    }
}
{
    [Route("api/admin/series")]
    [ApiController]
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
        public async Task<ActionResult<ApiResponse<long>>> CreateSeries([FromBody] CreateSeriesRequest createSeriesRequest)
        {
            var result = await _seriesService.CreateSeries(createSeriesRequest);
            return HandleResult(result);
        }

        // PUT: api/admin/series/{seriesId}
        // Updates series name or status
        [HttpPut("{seriesId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateSeries(
            long seriesId, 
            [FromBody] CreateSeriesRequest updateSeriesRequest)
        {
            // TODO: Implement the business logic to update this entity here. I will develop this part.
            // Implementation steps:
            // 1. Find series by ID
            // 2. Update Name if provided
            // 3. Update Status if provided
            // 4. Update UpdateDate and UpdateBy
            // 5. Save changes
            
            return Ok(new ApiResponse<string>(
                "Series update endpoint is ready but not yet implemented",
                null!
            ));
        }

        // Dvar result = await _seriesService.UpdateSeries(seriesId, updateSeriesRequest);
            return HandleResult(result/ 5. Save changes
            
            return Ok(new ApiResponse<string>(
                "Series deletion endpoint is ready but not yet implemented",
            var result = await _seriesService.DeleteSeries(seriesId);
            return HandleResult(result