namespace cellPhoneS_backend.DTOs.Requests;

public record UpdateSeriesRequest(
    long Id,
    long? BrandId,
    string? Name,
    string? Status
);
