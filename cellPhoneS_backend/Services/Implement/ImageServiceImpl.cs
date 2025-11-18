using System;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class ImageServiceImpl : ImageService
{
    public Task<ServiceResult<bool>> DeleteImage(long imageId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<byte[]>> GetImage(long imageId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> UpdateImage(long imageId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> UploadImage()
    {
        throw new NotImplementedException();
    }
}
