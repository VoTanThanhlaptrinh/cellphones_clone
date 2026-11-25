using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>Bộ lọc <c>GET /api/admin/audit-logs</c>: <c>Search</c> khớp email hoặc path; <c>Status</c> là mã HTTP (vd. 403).</summary>
public class AuditLogFilter : AdminListQuery
{
    public string? UserId { get; set; }
    public string? Resource { get; set; }
}
