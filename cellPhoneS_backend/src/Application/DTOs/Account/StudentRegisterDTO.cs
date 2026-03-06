namespace cellphones_backend.DTOs.Account;

public record class StudentRegisterDTO
(
    RegisterDTO Register,
    string TypeUser,
    string TypeSchool,
    string NameSchool,
    string IdStudent,
    string NameInCard,
    DateOnly ExpiredDateCard,
    string EmailSchool,
    byte[] FrontFaceCard,
    byte[] BehindFaceCard
);
