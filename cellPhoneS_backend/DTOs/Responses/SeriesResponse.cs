namespace cellPhoneS_backend.DTOs.Responses;

public record SeriesResponse(
    long Id,
    string? Name,
    long BrandId,
    string? BrandName,
    string? Status
);
