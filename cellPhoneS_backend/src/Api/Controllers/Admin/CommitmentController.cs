using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/commitments")]
    [ApiController]
    public class AdminCommitmentController : BaseController
    {
        private readonly CommitmentService _commitmentService;

        public AdminCommitmentController(CommitmentService commitmentService)
        {
            _commitmentService = commitmentService;
        }

        // POST: api/admin/commitments
        // Creates a single commitment for a product (e.g., "12-month warranty", "Free shipping")
        [HttpPost]
        public async Task<ActionResult<ApiResponse<long>>> CreateCommitment([FromBody] CreateCommitmentRequest createCommitmentRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _commitmentService.CreateCommitment(createCommitmentRequest, userId);
            return HandleResult(result);
        }

        // POST: api/admin/commitments/batch
        // Creates multiple commitments for a product in one request
        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<List<long>>>> CreateMultipleCommitments([FromBody] CreateMultipleCommitmentsRequest createMultipleRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<List<long>>("User not authenticated", new List<long>()));
            }
            
            var result = await _commitmentService.CreateMultipleCommitments(createMultipleRequest, userId);
            return HandleResult(result);
        }

        // PUT: api/admin/commitments/{commitmentId}
        // Updates the text of a commitment
        [HttpPut("{commitmentId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateCommitment(long commitmentId, [FromBody] string newContext)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _commitmentService.UpdateCommitment(commitmentId, newContext, userId);
            return HandleResult(result);
        }

        // DELETE: api/admin/commitments/{commitmentId}
        // Soft deletes a commitment (sets status to "deleted")
        [HttpDelete("{commitmentId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCommitment(long commitmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _commitmentService.DeleteCommitment(commitmentId, userId);
            return HandleResult(result);
        }
    }
}