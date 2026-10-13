using System;

namespace cellphones_backend.DTOs.Responses
{
    // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
    public record ReviewResponse(
        int Id,
        string UserId,
        string UserEmail,
        string UserName,
        long ProductId,
        int Rating,
        string? Comment,
        DateTime CreatedDate
    );
    // === END NEW FEATURE: PRODUCT REVIEWS ===
}
