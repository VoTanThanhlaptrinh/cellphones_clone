using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement
{
    public class EmailServiceImpl : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailServiceImpl(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var mailMessage = new MimeMessage();
            
            mailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            mailMessage.To.Add(new MailboxAddress("", toEmail));
            mailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = message };
            mailMessage.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]!), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["Password"]);
            await smtp.SendAsync(mailMessage);
            await smtp.DisconnectAsync(true);
        }
    }
}
