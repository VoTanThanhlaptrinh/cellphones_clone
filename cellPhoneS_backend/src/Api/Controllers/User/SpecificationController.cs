using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.User
{
    [Route("api/specifications")]
    [ApiController]
    public class SpecificationController : BaseController
    {
        private readonly SpecificationService _specificationService;

        public SpecificationController(SpecificationService specificationService)
        {
            _specificationService = specificationService;
        }

        // GET: api/specifications
        // Retrieves all specifications with their details for public viewing
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SpecificationDTO>>>> GetAllSpecifications()
        {
            var result = await _specificationService.GetAllSpecifications();
            return HandleResult(result);
        }

        // GET: api/specifications/{specificationId}
        // Retrieves a specific specification with its details
        [HttpGet("{specificationId}")]
        public async Task<ActionResult<ApiResponse<SpecificationDTO>>> GetSpecificationById(long specificationId)
        {
            var result = await _specificationService.GetSpecificationById(specificationId);
            return HandleResult(result);
        }
    }
}
