using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.User
{
    [Route("api/series")]
    [ApiController]
    public class SeriesController : BaseController
    {
        private readonly SeriesService _seriesService;

        public SeriesController(SeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        // GET: api/series/brand/{brandId}
        // Retrieves all series for a specific brand for public viewing
        [HttpGet("brand/{brandId}")]
        public async Task<ActionResult<ApiResponse<List<SeriesResponse>>>> GetSeriesByBrand(long brandId)
        {
            var result = await _seriesService.GetSeriesByBrand(brandId);
            return HandleResult(result);
        }

        // GET: api/series/{seriesId}
        // Retrieves series details by ID for public viewing
        [HttpGet("{seriesId}")]
        public async Task<ActionResult<ApiResponse<SeriesResponse>>> GetSeriesById(long seriesId)
        {
            var result = await _seriesService.GetSeriesById(seriesId);
            return HandleResult(result);
        }

        // GET: api/series
        // Retrieves all active paginated series across all brands for public viewing
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<SeriesResponse>>>> GetAllSeries(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = "active")
        {
            var result = await _seriesService.GetAllSeries(pageIndex, pageSize, status);
            return HandleResult(result);
        }
    }
}