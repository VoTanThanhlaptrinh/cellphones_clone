namespace cellPhoneS_backend.DTOs;

public record SpecificationDTO(string? Name, ICollection<SpecificationDetailDTO>? SpecDetails);
