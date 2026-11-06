using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Email;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers
{
    [Route("api/emails")]
    public class EmailController : BaseController
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<MailItemResponse>>> SendEmail(
            [FromBody] SendEmailRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _emailService.SendEmailAsync(request.To, request.Subject, request.Body, cancellationToken);
            return Ok(new ApiResponse<MailItemResponse>("Email sent successfully", result));
        }

        [HttpPost("send-bulk")]
        public async Task<ActionResult<ApiResponse<BulkMailResponse>>> SendBulkEmail(
            [FromBody] SendBulkEmailRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _emailService.SendBulkEmailAsync(request, cancellationToken);
            return Ok(new ApiResponse<BulkMailResponse>("Bulk email processed", result));
        }
    }
}
