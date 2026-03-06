using System;
using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.J2O;

public record ChangePasswordRequest([property: Required] string? CurrentPassword, [property: Required] string? NewPassword, [property: Required] string? ConfirmPassword);
