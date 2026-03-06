namespace cellPhoneS_backend.DTOs.Requests;

public record CreateSeriesRequest(
    long BrandId,
    string Name,
    string? Status = "active"
);
