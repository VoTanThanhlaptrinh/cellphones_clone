using System;

namespace cellPhoneS_backend.DTOs.Account;

public record UserResponse
{
    public string? Fullname { get; set; }
    public string? Phone { get; set; }
    public UserResponse(string Fullname, string Phone)
    {
        this.Fullname = Fullname;
        this.Phone = Phone;
    }
}
