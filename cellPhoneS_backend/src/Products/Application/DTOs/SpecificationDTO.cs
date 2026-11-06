namespace cellPhoneS_backend.DTOs;

public record SpecificationDTO(string? Name, ICollection<SpecificationDetailDTO>? SpecDetails, long? Id = null)
{
    public SpecificationDTO(string? name, ICollection<SpecificationDetailDTO>? specDetails) : this(name, specDetails, null) { }
}
