namespace cellPhoneS_backend.DTOs.Responses;

public record BrandResponse(
    long Id, 
    string Name, 
    string SlugName
);
