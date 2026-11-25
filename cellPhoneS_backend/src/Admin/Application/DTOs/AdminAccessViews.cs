namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Thông tin tài khoản quản trị đang đăng nhập, để UI ẩn/hiện menu và nút (backend vẫn là nơi quyết định).</summary>
public record AdminMeView(
    string Id,
    string? Email,
    string? FullName,
    List<string> Roles,
    List<string> Permissions);

public record PermissionView(string Code, string Group, string Description);

public record RoleView(
    string Id,
    string Name,
    string Status,
    bool IsSystem,
    int UserCount,
    List<string> Permissions);

public record AuditLogView(
    long Id,
    string UserId,
    string? UserEmail,
    string Method,
    string Path,
    string? Resource,
    string? ResourceId,
    int StatusCode,
    string? Payload,
    string? Ip,
    DateTime CreateDate);
