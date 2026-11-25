using cellPhoneS_backend.DTOs.Email;

namespace cellPhoneS_backend.Services.Interface
{
    public interface IEmailService
    {
        Task<MailItemResponse> SendEmailAsync(string toEmail, string subject, string message, CancellationToken cancellationToken = default);
        Task<BulkMailResponse> SendBulkEmailAsync(SendBulkEmailRequest request, CancellationToken cancellationToken = default);
    }
}
