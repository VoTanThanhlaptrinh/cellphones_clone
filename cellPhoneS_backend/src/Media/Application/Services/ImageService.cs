using cellPhoneS_backend.DTOs;

namespace cellphones_backend.Services;

public interface ImageService
{
    // Admin operations
    Task<long> CreateImage(CreateImageRequest request, string userId);
    Task<long> UploadImage(IFormFile file, string? alt, string userId);
    Task DeleteImage(long imageId, string userId);
    
    // Public/User operations
    Task<ImageDTO> GetImage(long imageId);
}
