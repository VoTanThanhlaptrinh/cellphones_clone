namespace cellPhoneS_backend.DTOs.Requests;

public record UpdateStoreHouseRequest(
    long Id,
    string? Street,
    string? District,
    string? City,
    string? Status
);
