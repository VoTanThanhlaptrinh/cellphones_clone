namespace cellPhoneS_backend.DTOs.Responses;

public record DemandResponse(
    long Id, 
    string Name, 
    string SlugName
);
