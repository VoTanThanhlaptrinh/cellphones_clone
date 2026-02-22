using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.User
{
    [Route("api/commitments")]
    [ApiController]
    public class CommitmentController : BaseController
    {
        private readonly CommitmentService _commitmentService;

        public CommitmentController(CommitmentService commitmentService)
        {
            _commitmentService = commitmentService;
        }

        // GET: api/commitments/product/{productId}
        // Retrieves all commitments for a specific product for public viewing
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetProductCommitments(long productId)
        {
            var result = await _commitmentService.GetProductCommitments(productId);
            return HandleResult(result);
        }
    }
}
