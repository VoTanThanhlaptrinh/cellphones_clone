namespace cellPhoneS_backend.DTOs.Requests;

public record CreateRoleRequest(string Name, List<string>? Permissions);

public record UpdateRoleRequest(string Id, string Name);

public record UpdateRolePermissionsRequest(List<string> Permissions);
