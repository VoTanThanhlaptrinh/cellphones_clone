namespace cellPhoneS_backend.Services.Interface;

public record FilePresignResult(string TempKey, string UploadUrl, int ExpiresIn, string? PreviewUrl);

public record FileConfirmResult(string Key, string Visibility, string? Url);

/// <summary>
/// Client của file-service (presign → PUT lên R2 → confirm). Ném <see cref="BadRequestException"/> khi dữ liệu bị từ chối
/// và <see cref="InternalServerException"/> khi service lỗi hoặc không kết nối được.
/// </summary>
public interface IFileServiceClient
{
    Task<FilePresignResult> PresignAsync(string context, string fileName, string mimeType, long size);

    /// <summary>PUT thẳng lên presigned URL (không gửi API key). Dùng khi chính backend giữ nội dung file.</summary>
    Task UploadToPresignedUrlAsync(string uploadUrl, byte[] content, string mimeType);

    Task<FileConfirmResult> ConfirmAsync(string tempKey);

    Task DeleteAsync(string key);
}
