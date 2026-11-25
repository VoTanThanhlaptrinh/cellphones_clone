namespace cellPhoneS_backend.DTOs.Account;

// Thông tin tài khoản trả cho chính người dùng (không bao giờ trả entity User: lộ PasswordHash, SecurityStamp...)
public record UserResponse(string Id, string? Email, string? Fullname, string? Phone, DateOnly Birth);
