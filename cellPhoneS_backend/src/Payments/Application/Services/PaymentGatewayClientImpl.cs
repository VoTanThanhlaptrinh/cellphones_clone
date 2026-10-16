
using System.Text.Json;
using System.Text.Json.Serialization;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement;

public class PaymentGatewayClientImpl : IPaymentGatewayClient
{
    private const int MaxCheckoutAttempts = 3;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _http;
    private readonly ILogger<PaymentGatewayClientImpl> _logger;

    public PaymentGatewayClientImpl(HttpClient http, ILogger<PaymentGatewayClientImpl> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<GatewayPayment> Checkout(long orderId, long amount, string provider, string idempotencyKey)
    {
        // Thử lại luôn dùng đúng Idempotency-Key và body nên gateway không bao giờ tạo hai payment.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/payments/checkout")
                {
                    Content = JsonContent.Create(new { orderId, amount, currency = "VND", provider }, options: Json)
                };
                request.Headers.Add("Idempotency-Key", idempotencyKey);
                return await Send(request);
            }
            catch (PaymentGatewayException ex) when (attempt < MaxCheckoutAttempts && IsRetryable(ex))
            {
                _logger.LogWarning("Checkout đơn {OrderId} lỗi tạm thời ({Code}), thử lại lần {Attempt}", orderId, ex.Code, attempt);
                await Task.Delay(TimeSpan.FromSeconds(ex.RetryAfterSeconds ?? Math.Pow(2, attempt - 1)));
            }
        }
    }

    public async Task<GatewayPayment?> GetStatus(long orderId)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/payments/status?orderId={orderId}");
            return await Send(request);
        }
        catch (PaymentGatewayException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<GatewayPayment> Cancel(long orderId, string? reason)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/payments/cancel")
        {
            Content = JsonContent.Create(new { orderId, reason }, options: Json)
        };
        return await Send(request);
    }

    private static bool IsRetryable(PaymentGatewayException ex) =>
        ex.Status == 0 || ex.Status >= 500 || ex.Code == "IDEMPOTENCY_IN_PROGRESS";

    private async Task<GatewayPayment> Send(HttpRequestMessage request)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new PaymentGatewayException(0, "GATEWAY_UNREACHABLE", "Không kết nối được cổng thanh toán.");
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<PaymentResponse>(Json)
                    ?? throw new PaymentGatewayException((int)response.StatusCode, "INVALID_RESPONSE", "Cổng thanh toán trả dữ liệu rỗng.");
                return body.ToGatewayPayment();
            }

            var error = await TryReadError(response);
            var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds;
            _logger.LogWarning("Gateway trả {Status} {Code} traceId={TraceId}", (int)response.StatusCode, error?.Code, error?.TraceId);
            throw new PaymentGatewayException((int)response.StatusCode, error?.Code ?? $"HTTP_{(int)response.StatusCode}",
                error?.Message ?? "Cổng thanh toán từ chối yêu cầu.", error?.TraceId) { RetryAfterSeconds = retryAfter };
        }
    }

    private static async Task<ErrorResponse?> TryReadError(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ErrorResponse>(Json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private record ErrorResponse(string? Code, string? Message, string? TraceId);

    private record PaymentResponse(
        long PaymentId, long OrderId, decimal TotalAmount, string Provider, string Status,
        string? TransactionId, string? TransactionStatus, string? CheckoutUrl, DateTimeOffset? ExpiresAt, DateTimeOffset? PaidAt,
        GatewayPaymentInfo? PaymentInfo)
    {
        public GatewayPayment ToGatewayPayment() =>
            new(PaymentId, OrderId, TotalAmount, Provider, Status, TransactionId, TransactionStatus, CheckoutUrl, ExpiresAt, PaidAt, PaymentInfo);
    }
}
