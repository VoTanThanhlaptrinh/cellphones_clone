using System;
using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public record ImageDTO(string? OriginUrl, string? MimeType, string? Name, string? Alt);
