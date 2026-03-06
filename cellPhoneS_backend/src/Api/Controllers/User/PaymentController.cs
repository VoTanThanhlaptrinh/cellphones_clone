using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace cellphones_backend.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly PayOSService _payOSService;
        public PaymentController(IPaymentRepository paymentRepository, PayOSService payOSService)
        {
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] Webhook webhookData)
        {
            try
            {
                var verifiedData = await _payOSService.VerifyPayment(webhookData);
                if(verifiedData == true)
                {
                    return Ok("OK");
                }
                else
                {
                    return BadRequest("Invalid webhook");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Webhook không hợp lệ: {ex.Message}");
                return BadRequest("Invalid webhook");
            }
        }
        [HttpGet("qr-payment/{orderId}")]
        public Task<ActionResult<ApiResponse<string>>> GenerateQRPaymentLink(long orderId)
        {
            return null!; // TODO: implement QR code payment link generation
        }

    }
}
