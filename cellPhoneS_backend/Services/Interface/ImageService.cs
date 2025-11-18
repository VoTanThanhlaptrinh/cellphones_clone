using System;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface ImageService
{
    Task<ServiceResult<string>> UploadImage();
    Task<ServiceResult<byte[]>> GetImage(long imageId);
    Task<ServiceResult<bool>> DeleteImage(long imageId);
    Task<ServiceResult<string>> UpdateImage(long imageId);
}
