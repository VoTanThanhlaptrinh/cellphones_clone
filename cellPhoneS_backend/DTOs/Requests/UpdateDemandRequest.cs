namespace cellPhoneS_backend.DTOs.Requests;

public record UpdateDemandRequest(
    long Id,
    long? CategoryId,
    string? Name,
    string? Status
);
