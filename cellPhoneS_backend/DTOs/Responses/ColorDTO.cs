using System;
using cellphones_backend.Models;

namespace cellPhoneS_backend.DTOs.Responses;

public record ColorDTO(long colorId,string? name,ImageDTO? image);
