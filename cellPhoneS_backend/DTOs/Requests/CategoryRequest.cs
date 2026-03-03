using System;
using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.J2O;

public record CategoryRequest([property: Required] string? CategoryName);
