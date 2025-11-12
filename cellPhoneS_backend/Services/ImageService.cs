using System;
using cellphones_backend.DTOs.Responses;

namespace cellphones_backend.Services;

public interface ImageService
{
    public ApiResponse<string> UploadImage();
    public ApiResponse<byte[]> GetImage(long imageId);
    public ApiResponse<bool> DeleteImage(long imageId);
    public ApiResponse<string> UpdateImage(long imageId);
}
