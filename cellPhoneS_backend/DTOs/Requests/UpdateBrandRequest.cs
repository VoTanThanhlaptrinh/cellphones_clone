namespace cellPhoneS_backend.DTOs.Requests;

public record UpdateBrandRequest(
    long Id,
    long? CategoryId,
    string? Name,
    long? ImageId,
    string? Status
);
