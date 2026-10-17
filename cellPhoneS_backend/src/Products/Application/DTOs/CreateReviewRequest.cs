using System.ComponentModel.DataAnnotations;

namespace cellphones_backend.DTOs.Requests
{
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    public record CreateReviewRequest(
        [Required] long ProductId,
        [Range(1, 5)] int Rating,
        string? Comment
    );
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
