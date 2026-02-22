using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
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
        public async Task<ActionResult<ApiResponse<List<SeriesView>>>> GetSeriesByBrand(long brandId)
        {
            var result = await _seriesService.GetSeriesByBrand(brandId);
            return HandleResult(result);
        }

        // GET: api/series/{seriesId}
        // Retrieves series details by ID for public viewing
        [HttpGet("{seriesId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetSeriesById(long seriesId)
        {
            // TODO: Implement the business logic to retrieve series details here. I will develop this part.
            
            return Ok(new ApiResponse<object>(SeriesView>>> GetSeriesById(long seriesId)
        {
            var result = await _seriesService.GetSeriesById(seriesId);
            return HandleResult(result