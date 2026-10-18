namespace cellPhoneS_backend.DTOs.Responses;

public record AdminReviewView(
    int Id,
    long ProductId,
    string? ProductName,
    string UserId,
    string? UserEmail,
    string? UserName,
    int Rating,
    string? Comment,
    string Status,
    DateTime CreatedDate);
