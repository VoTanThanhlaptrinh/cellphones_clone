using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>Nhật ký các thao tác ghi dưới /api/admin/* (ghi tự động bởi AdminAuditFilter).</summary>
    [Route("api/admin/audit-logs")]
    [ApiController]
    [RequirePermission(Permissions.AuditRead)]
    public class AdminAuditLogController : BaseController
    {
        private readonly IAuditLogService _auditLogService;

        public AdminAuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: api/admin/audit-logs?pageIndex=1&pageSize=20&userId=&resource=orders&status=200&search=&from=&to=
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AuditLogView>>>> GetAuditLogs([FromQuery] AuditLogFilter filter)
        {
            var result = await _auditLogService.GetAuditLogs(filter);
            return Ok(new ApiResponse<PagedResult<AuditLogView>>("Success", result));
        }
    }
}
