using System.Text.Json;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement;

public class FileServiceClientImpl : IFileServiceClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly ILogger<FileServiceClientImpl> _logger;

    public FileServiceClientImpl(HttpClient http, IConfiguration configuration, ILogger<FileServiceClientImpl> logger)
    {
        _http = http;
        _apiKey = configuration[FileServiceConfigKeys.ApiKey] ?? "";
        _logger = logger;
    }

    public async Task<FilePresignResult> PresignAsync(string context, string fileName, string mimeType, long size)
    {
        var body = new { context, file = new { fileName, mimeType, size } };
        return await Send<FilePresignResult>(HttpMethod.Post, "api/v1/files/presign", body);
    }

    public async Task UploadToPresignedUrlAsync(string uploadUrl, byte[] content, string mimeType)
    {
        // Không gắn x-api-key: URL đã được ký, header lạ chỉ làm R2 từ chối.
        using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl) { Content = new ByteArrayContent(content) };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        try
        {
            using var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PUT lên R2 trả {Status}", (int)response.StatusCode);
                throw new InternalServerException("Không tải được ảnh lên kho lưu trữ.");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new InternalServerException("Không kết nối được kho lưu trữ ảnh.", ex);
        }
    }

    public async Task<FileConfirmResult> ConfirmAsync(string tempKey)
        => await Send<FileConfirmResult>(HttpMethod.Post, "api/v1/files/confirm", new { tempKey });

    public async Task DeleteAsync(string key)
        => await Send<JsonElement>(HttpMethod.Delete, "api/v1/files", new { key });

    private async Task<T> Send<T>(HttpMethod method, string path, object body)
    {
        using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body, options: Json) };
        request.Headers.Add("x-api-key", _apiKey);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Không kết nối được file-service ({Path})", path);
            throw new InternalServerException("Không kết nối được dịch vụ lưu trữ ảnh.", ex);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                var envelope = await response.Content.ReadFromJsonAsync<Envelope<T>>(Json);
                if (envelope is null || envelope.Data is null)
                    throw new InternalServerException("Dịch vụ lưu trữ ảnh trả dữ liệu rỗng.");
                return envelope.Data;
            }

            var message = await TryReadMessage(response);
            _logger.LogWarning("file-service trả {Status} cho {Path}: {Message}", (int)response.StatusCode, path, message);
            if ((int)response.StatusCode == 400)
                throw new BadRequestException(message ?? "Yêu cầu upload ảnh không hợp lệ.");
            throw new InternalServerException("Dịch vụ lưu trữ ảnh đang lỗi, vui lòng thử lại sau.");
        }
    }

    private static async Task<string?> TryReadMessage(HttpResponseMessage response)
    {
        try
        {
            return (await response.Content.ReadFromJsonAsync<ErrorEnvelope>(Json))?.Message;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private record Envelope<T>(bool Success, T? Data);

    private record ErrorEnvelope(bool Success, string? Message);
}
