using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

public interface IAuditLogService
{
    /// <summary>Ghi một dòng audit (tự điền <c>UserEmail</c> và <c>CreateDate</c>).</summary>
    Task Write(AuditLog entry);
    Task<PagedResult<AuditLogView>> GetAuditLogs(AuditLogFilter filter);
}
