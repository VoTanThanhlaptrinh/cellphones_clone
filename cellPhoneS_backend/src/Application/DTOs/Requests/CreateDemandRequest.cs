namespace cellPhoneS_backend.DTOs.Requests;

public record CreateDemandRequest(
    long CategoryId,
    string Name,
    string? Status = "active"
);
