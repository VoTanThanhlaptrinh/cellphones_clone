using System;
using cellPhoneS_backend.Services.Interface;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

using PayOS;
namespace cellPhoneS_backend.Services.Implement;

public class PayOSServiceImpl : PayOSService
{
    private readonly string PS_CliENT_ID;
    private readonly string PS_API_KEY;
    private readonly string PS_CHECKSUM_KEY;
    private readonly string PS_CALL_BACK_URL;
    private readonly string PS_CANCEL_URL;
    private readonly PayOSClient _payOSClient;
    private readonly IConfiguration _configuration;
    public PayOSServiceImpl(IConfiguration configuration)
    {
        _configuration = configuration;
        PS_CliENT_ID = _configuration["PayOS:clientId"]!;
        PS_API_KEY = _configuration["PayOS:apiKey"]!;
        PS_CHECKSUM_KEY = _configuration["PayOS:checksumKey"]!;
        PS_CALL_BACK_URL = _configuration["PayOS:callBackUrl"]!;
        PS_CANCEL_URL = _configuration["PayOS:cancelUrl"]!;
        _payOSClient = new PayOSClient(
            PS_CliENT_ID,
            PS_API_KEY,
            PS_CHECKSUM_KEY
        );
    }
    public async Task<string> CreatePaymentLink(long orderId, long amount, string description)
    {
        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderId,
            Amount = amount,
            Description = description,
            CancelUrl = PS_CANCEL_URL,
            ReturnUrl = PS_CALL_BACK_URL,
        };

        var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);
        return paymentLink.CheckoutUrl;
    }

    public async Task<ServiceResult<string>> GenerateQRCode(long orderId, long amount, string description)
    {
        try
        {
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderId,
                Amount = amount,
                Description = description,
                CancelUrl = PS_CANCEL_URL,
                ReturnUrl = PS_CALL_BACK_URL,
            };

            var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);
            return ServiceResult<string>.Success(paymentLink.QrCode, "QR code generated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating QR code: {ex.Message}");
            return ServiceResult<string>.Fail("Failed to generate QR code");
        }
    }

    public async Task<bool> VerifyPayment(Webhook webhook)
    {
        try
        {
            var verifiedData = await _payOSClient.Webhooks.VerifyAsync(webhook);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying payment: {ex.Message}");
            return false;
        }
    }
}
