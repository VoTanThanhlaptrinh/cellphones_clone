namespace cellPhoneS_backend.DTOs;

public record SpecificationDetailDTO(string? Name, string? Value, long? Id = null)
{
    public SpecificationDetailDTO(string? name, string? value) : this(name, value, null) { }
}
