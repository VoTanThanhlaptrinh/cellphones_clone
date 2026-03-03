namespace cellPhoneS_backend.DTOs.Requests;

public record CreateBrandRequest(
    long CategoryId,
    string Name,
    long? ImageId,
    string? Status = "active"
);
