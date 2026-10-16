using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    /// <summary>Thanh toán online của đơn hàng, thực hiện qua payment gateway (payment-module).</summary>
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly OrderService _orderService;
        public PaymentController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/payments/checkout — tạo (hoặc lấy lại) link thanh toán online của đơn
        [HttpPost("checkout")]
        public async Task<ActionResult<ApiResponse<OnlinePaymentView>>> Checkout([FromBody] OnlineCheckoutRequest payload)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.StartOnlinePayment(userId!, payload.OrderId, payload.Provider);
            return Ok(new ApiResponse<OnlinePaymentView>("Success", result));
        }

        // GET: api/payments/{id} — giao dịch theo mã (URL trang thanh toán giữ mã này nên tải lại không mất giao dịch); còn chờ thì hỏi gateway
        [HttpGet("{id:long}")]
        public async Task<ActionResult<ApiResponse<OnlinePaymentView>>> GetPayment(long id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.GetOnlinePayment(userId!, id);
            return Ok(new ApiResponse<OnlinePaymentView>("Success", result));
        }
    }
}
