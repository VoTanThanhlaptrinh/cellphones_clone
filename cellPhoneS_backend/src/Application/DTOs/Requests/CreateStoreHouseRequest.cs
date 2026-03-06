namespace cellPhoneS_backend.DTOs.Requests;

public record CreateStoreHouseRequest(
    string Street,
    string District,
    string City,
    string? Status = "active"
);
