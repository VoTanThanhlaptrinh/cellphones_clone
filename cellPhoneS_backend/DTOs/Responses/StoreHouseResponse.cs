namespace cellPhoneS_backend.DTOs.Responses;

public record StoreHouseResponse(
    long Id,
    string? Street,
    string? District,
    string? City,
    string? Status
);
