using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs.Account;

public record UpdateMeRequest(
    [property: Required, MaxLength(100)] string? Fullname,
    [property: Required] DateOnly? Birth,
    [property: Required, RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số")] string? Phone
);
