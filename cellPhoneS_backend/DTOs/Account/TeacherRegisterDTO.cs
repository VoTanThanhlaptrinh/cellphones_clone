namespace cellphones_backend.DTOs.Account;

public record class TeacherRegisterDTO
(
    RegisterDTO Register,
    string TypeUser,
    string TypeSchool,
    string NameSchool,
    string IdTeacher,
    string NameInCard,
    string EmailSchool,
    byte[] FrontFaceCard,
    byte[] BehindFaceCard
);
