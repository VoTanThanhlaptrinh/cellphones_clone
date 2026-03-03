using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.User
{
    [Route("api/demands")]
    [ApiController]
    public class DemandController : BaseController
    {
        private readonly DemandService _demandService;

        public DemandController(DemandService demandService)
        {
            _demandService = demandService;
        }

        // GET: api/demands/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<DemandResponse>>>> GetDemandsByCategory(long categoryId)
        {
            var result = await _demandService.GetDemandsByCategory(categoryId);
            return HandleResult(result);
        }

        // GET: api/demands/{demandId}
        [HttpGet("{demandId}")]
        public async Task<ActionResult<ApiResponse<DemandResponse>>> GetDemandById(long demandId)
        {
            var result = await _demandService.GetDemandById(demandId);
            return HandleResult(result);
        }

        // GET: api/demands
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<DemandResponse>>>> GetAllDemands(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = "active")
        {
            var result = await _demandService.GetAllDemands(pageIndex, pageSize, status);
            return HandleResult(result);
        }
    }
}
