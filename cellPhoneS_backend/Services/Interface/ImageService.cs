using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;

namespace cellphones_backend.Services;

public interface ImageService
{
    // Admin operations
    Task<ServiceResult<long>> CreateImage(CreateImageRequest request, string userId);
    Task<ServiceResult<long>> UploadImage(IFormFile file, string? alt, string userId);
    Task<ServiceResult<string>> DeleteImage(long imageId, string userId);
    
    // Public/User operations
    Task<ServiceResult<ImageDTO>> GetImage(long imageId);
}
