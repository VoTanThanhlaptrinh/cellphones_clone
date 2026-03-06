using System;
using PayOS.Models;
using PayOS.Models.Webhooks;

namespace cellPhoneS_backend.Services.Interface;

public interface PayOSService
{
    Task<string> CreatePaymentLink(long orderId, long amount, string description);
    Task<bool> VerifyPayment(Webhook webhook);
    Task<ServiceResult<string>> GenerateQRCode(long orderId, long amount, string description);
}
