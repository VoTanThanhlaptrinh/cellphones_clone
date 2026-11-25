using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using cellPhoneS_backend.DTOs.Email;
using cellPhoneS_backend.Services;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement
{
    public class EmailServiceImpl : IEmailService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailServiceImpl> _logger;

        public EmailServiceImpl(HttpClient httpClient, ILogger<EmailServiceImpl> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MailItemResponse> SendEmailAsync(string toEmail, string subject, string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || !IsValidEmail(toEmail))
            {
                throw new BadRequestException("Địa chỉ email người nhận không hợp lệ.");
            }
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new BadRequestException("Tiêu đề email không được để trống.");
            }
            if (subject.Length > 200)
            {
                throw new BadRequestException("Tiêu đề email không được vượt quá 200 ký tự.");
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new BadRequestException("Nội dung email không được để trống.");
            }
            if (message.Length > 20000)
            {
                throw new BadRequestException("Nội dung email không được vượt quá 20000 ký tự.");
            }

            var payload = new SendEmailRequest(toEmail.Trim(), subject, message);
            return await PostMailAsync<MailItemResponse>("mails/send", payload, cancellationToken);
        }

        public async Task<BulkMailResponse> SendBulkEmailAsync(SendBulkEmailRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new BadRequestException("Dữ liệu yêu cầu gửi email không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                throw new BadRequestException("Tiêu đề email không được để trống.");
            }
            if (request.Subject.Length > 200)
            {
                throw new BadRequestException("Tiêu đề email không được vượt quá 200 ký tự.");
            }
            if (string.IsNullOrWhiteSpace(request.Body))
            {
                throw new BadRequestException("Nội dung email không được để trống.");
            }
            if (request.Body.Length > 20000)
            {
                throw new BadRequestException("Nội dung email không được vượt quá 20000 ký tự.");
            }
            if (request.Recipients == null || request.Recipients.Count == 0)
            {
                throw new BadRequestException("Danh sách người nhận không được để trống.");
            }
            if (request.Recipients.Count > 100)
            {
                throw new BadRequestException("Số lượng người nhận không được vượt quá 100.");
            }
            for (var i = 0; i < request.Recipients.Count; i++)
            {
                var recipient = request.Recipients[i];
                if (recipient == null || !IsValidEmail(recipient.To))
                {
                    throw new BadRequestException($"Email người nhận tại vị trí {i + 1} không hợp lệ: '{recipient?.To}'.");
                }
            }

            return await PostMailAsync<BulkMailResponse>("mails/send-bulk", request, cancellationToken);
        }

        private async Task<T> PostMailAsync<T>(string endpoint, object payload, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            try
            {
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = JsonContent.Create(payload, options: JsonOptions)
                };
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi kết nối tới Mail Service khi gọi {Endpoint}", endpoint);
                throw new BadGatewayException("Không thể kết nối tới Mail Service: " + ex.Message, ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout khi kết nối tới Mail Service khi gọi {Endpoint}", endpoint);
                throw new BadGatewayException("Timeout khi kết nối tới Mail Service.", ex);
            }

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    return DeserializeResponse<T>(responseBody);
                }

                var errorMessage = await ReadErrorMessageAsync(response, cancellationToken);
                _logger.LogWarning("Mail Service trả về mã lỗi {StatusCode}: {ErrorMessage}", (int)response.StatusCode, errorMessage);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new BadRequestException(errorMessage);

                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedException(errorMessage);

                    case HttpStatusCode.Forbidden:
                        throw new ForbiddenException(errorMessage);

                    case HttpStatusCode.BadGateway:
                        throw new BadGatewayException(errorMessage);

                    default:
                        if ((int)response.StatusCode >= 500)
                        {
                            throw new BadGatewayException(errorMessage);
                        }
                        throw new InternalServerException(errorMessage);
                }
            }
        }

        private static T DeserializeResponse<T>(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("data", out var dataProp))
            {
                return DeserializeDirect<T>(json);
            }

            if (root.TryGetProperty("total", out _) || root.TryGetProperty("id", out _))
            {
                return DeserializeDirect<T>(json);
            }

            var dataResult = JsonSerializer.Deserialize<T>(dataProp.GetRawText(), JsonOptions);
            if (dataResult != null)
            {
                return dataResult;
            }

            return DeserializeDirect<T>(json);
        }

        private static T DeserializeDirect<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                ?? throw new InternalServerException("Dịch vụ email trả về kết quả rỗng.");
        }

        private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return $"Mail service responded with status {(int)response.StatusCode} ({response.ReasonPhrase})";
            }

            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return content;
                }

                var message = root.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String
                    ? msgProp.GetString()
                    : null;

                var error = root.TryGetProperty("error", out var errProp) && errProp.ValueKind == JsonValueKind.String
                    ? errProp.GetString()
                    : null;

                var issues = ExtractIssues(root);
                if (!string.IsNullOrWhiteSpace(issues))
                {
                    return !string.IsNullOrWhiteSpace(message) ? $"{message}: {issues}" : issues;
                }

                if (!string.IsNullOrWhiteSpace(error) && !string.IsNullOrWhiteSpace(message) && error != message)
                {
                    return IsGenericHttpError(error) ? message : error;
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    return error;
                }

                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }
            catch (JsonException)
            {
                // Nội dung không phải JSON (VD: HTML báo lỗi gateway)
            }

            return content;
        }

        private static string? ExtractIssues(JsonElement root)
        {
            if (!root.TryGetProperty("issues", out var issuesProp) || issuesProp.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var parts = new List<string>();
            foreach (var item in issuesProp.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var str = item.GetString();
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        parts.Add(str);
                    }
                    continue;
                }

                if (item.ValueKind == JsonValueKind.Object)
                {
                    var itemMsg = item.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String
                        ? m.GetString()
                        : null;
                    var itemPath = item.TryGetProperty("path", out var p) && p.ValueKind == JsonValueKind.String
                        ? p.GetString()
                        : null;

                    if (!string.IsNullOrWhiteSpace(itemPath) && !string.IsNullOrWhiteSpace(itemMsg))
                    {
                        parts.Add($"{itemPath}: {itemMsg}");
                    }
                    else if (!string.IsNullOrWhiteSpace(itemMsg))
                    {
                        parts.Add(itemMsg);
                    }
                }
            }

            if (parts.Count == 0)
            {
                return null;
            }

            return string.Join("; ", parts);
        }

        private static bool IsGenericHttpError(string text)
        {
            return text.Equals("Bad Request", StringComparison.OrdinalIgnoreCase)
                || text.Equals("Unauthorized", StringComparison.OrdinalIgnoreCase)
                || text.Equals("Forbidden", StringComparison.OrdinalIgnoreCase)
                || text.Equals("Bad Gateway", StringComparison.OrdinalIgnoreCase)
                || text.Equals("Internal Server Error", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var trimmed = email.Trim();
            if (!trimmed.Contains('@')) return false;
            return System.Net.Mail.MailAddress.TryCreate(trimmed, out var addr) && addr.Address == trimmed;
        }
    }
}
