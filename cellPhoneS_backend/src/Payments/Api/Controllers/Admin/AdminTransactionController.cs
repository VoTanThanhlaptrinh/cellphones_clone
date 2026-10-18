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
    /// <summary>Giao dịch thanh toán đã ghi (chỉ đọc).</summary>
    [Route("api/admin/transactions")]
    [ApiController]
    [RequirePermission(Permissions.TransactionRead)]
    public class AdminTransactionController : BaseController
    {
        private readonly IAdminTransactionService _adminTransactionService;

        public AdminTransactionController(IAdminTransactionService adminTransactionService)
        {
            _adminTransactionService = adminTransactionService;
        }

        // GET: api/admin/transactions?provider=PAYOS&status=success&from=2026-10-01&to=2026-10-31&orderId=15
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<AdminTransactionView>>>> GetTransactions([FromQuery] AdminTransactionFilter filter)
        {
            var result = await _adminTransactionService.GetTransactions(filter);
            return Ok(new ApiResponse<PagedResult<AdminTransactionView>>("Success", result));
        }
    }
}
