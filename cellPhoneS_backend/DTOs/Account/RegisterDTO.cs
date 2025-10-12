namespace cellphones_backend.DTOs.Account;

public record class RegisterDTO
(
    string FullName,
    DateOnly BirthDay,
    string Phone,
    string Email,
    string Password,
    string ConfPassword
);
