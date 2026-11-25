using cellPhoneS_backend.DTOs;

namespace cellphones_backend.Services;

public interface ImageService
{
    // Admin operations
    Task<long> CreateImage(CreateImageRequest request, string userId);
    /// <summary>Bước 1 của upload file: xin link PUT thẳng lên R2 từ file-service.</summary>
    Task<PresignImageUploadResponse> PresignUpload(PresignImageUploadRequest request);
    /// <summary>Bước 3: xác nhận file đã PUT lên R2 rồi lưu bản ghi ảnh.</summary>
    Task<long> ConfirmUpload(ConfirmImageUploadRequest request, string userId);
    Task DeleteImage(long imageId, string userId);
    
    // Public/User operations
    Task<ImageDTO> GetImage(long imageId);
    Task<List<ImageDTO>> GetAllImages();
}
