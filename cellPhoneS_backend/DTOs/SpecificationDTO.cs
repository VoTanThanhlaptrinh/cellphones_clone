using System;

namespace cellPhoneS_backend.DTOs;
using System.ComponentModel.DataAnnotations;

public record SpecificationDTO(string? Name, ICollection<SpecificationDetailDTO>? SpecDetails);
