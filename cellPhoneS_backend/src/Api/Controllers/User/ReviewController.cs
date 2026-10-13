using cellphones_backend.Services;
using cellPhoneS_backend.Controllers;
using cellphones_backend.DTOs.Requests;
using cellphones_backend.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace cellphones_backend.Controllers.User
{
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // POST: api/reviews
        // Submits a new product review. Requires authentication.
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<ReviewResponse>("User is not logged in", default!));
            }

            var result = await _reviewService.CreateReviewAsync(request, userId);
            return Ok(new ApiResponse<ReviewResponse>("Review submitted successfully", result));
        }

        // GET: api/reviews/product/{productId}
        // Retrieves all reviews for a product for public viewing.
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetReviewsByProduct(long productId)
        {
            var result = await _reviewService.GetReviewsByProductIdAsync(productId);
            return Ok(new ApiResponse<List<ReviewResponse>>("Reviews retrieved successfully", result));
        }

        // GET: api/reviews/product/{productId}/average
        // Retrieves the average rating (1.0 - 5.0) for a specific product.
        [HttpGet("product/{productId}/average")]
        public async Task<ActionResult<ApiResponse<double>>> GetAverageRating(long productId)
        {
            var result = await _reviewService.GetAverageRatingAsync(productId);
            return Ok(new ApiResponse<double>("Average rating retrieved successfully", result));
        }
    }
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
