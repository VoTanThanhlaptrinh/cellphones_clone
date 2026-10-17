using System.Security.Claims;
using System.Text;
using System.Text.Json;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.Services;
using cellPhoneS_backend.Services.Implement;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly OrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;
        public PaymentController(OrderService orderService, IConfiguration configuration, ILogger<PaymentController> logger)
        {
            _orderService = orderService;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/payments/callback — payment gateway báo payment của đơn đã có kết quả cuối (PAID, FAILED, EXPIRED, CANCELLED, REVIEW_REQUIRED).
        // Không đăng nhập: xác thực bằng chữ ký HMAC trên body thô (xem RouteConfig). Callback chỉ là gợi ý, kết quả thật lấy lại từ gateway.
        [HttpPost("callback")]
        [RequestSizeLimit(16 * 1024)]
        public async Task<ActionResult<ApiResponse<string>>> Callback()
        {
            var secret = _configuration[PaymentConfigKeys.CallbackSecret];
            if (string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogError("Chưa cấu hình {Key}: không nhận được callback thanh toán", PaymentConfigKeys.CallbackSecret);
                throw new InternalServerException("Callback thanh toán chưa được cấu hình.");
            }

            using var buffer = new MemoryStream();
            await Request.Body.CopyToAsync(buffer);
            var body = buffer.ToArray();

            if (!PaymentCallbackSignature.IsValid(secret,
                    Request.Headers[PaymentCallbackHeaders.Timestamp].ToString(),
                    Request.Headers[PaymentCallbackHeaders.Signature].ToString(),
                    body, DateTimeOffset.UtcNow))
            {
                _logger.LogWarning("Callback thanh toán sai chữ ký hoặc timestamp quá hạn (eventId={EventId})", Request.Headers[PaymentCallbackHeaders.EventId].ToString());
                throw new UnauthorizedException("Chữ ký callback không hợp lệ.");
            }

            PaymentCallbackPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<PaymentCallbackPayload>(Encoding.UTF8.GetString(body), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            catch (JsonException)
            {
                throw new BadRequestException("Nội dung callback không hợp lệ.");
            }
            if (payload?.Payment == null) throw new BadRequestException("Nội dung callback không hợp lệ.");

            _logger.LogInformation("Callback thanh toán {EventType} eventId={EventId} đơn {OrderId} payment {PaymentId} status={Status}",
                payload.EventType, payload.EventId, payload.Payment.OrderId, payload.Payment.PaymentId, payload.Payment.Status);
            await _orderService.HandlePaymentCallback(payload.Payment.OrderId);
            return Ok(new ApiResponse<string>("Success", ""));
        }

        // POST: api/payments/checkout — tạo (hoặc lấy lại) link thanh toán online của đơn
        [HttpPost("checkout")]
        public async Task<ActionResult<ApiResponse<OnlinePaymentView>>> Checkout([FromBody] OnlineCheckoutRequest payload)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.StartOnlinePayment(userId!, payload.OrderId, payload.Provider);
            return Ok(new ApiResponse<OnlinePaymentView>("Success", result));
        }

        // GET: api/payments/order/{orderId} — payment của đơn, lấy từ gateway (URL trang thanh toán giữ mã đơn nên tải lại không mất giao dịch)
        [HttpGet("order/{orderId:long}")]
        public async Task<ActionResult<ApiResponse<OnlinePaymentView>>> GetPayment(long orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.GetOnlinePayment(userId!, orderId);
            return Ok(new ApiResponse<OnlinePaymentView>("Success", result));
        }
    }
}
