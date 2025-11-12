using System;

namespace cellPhoneS_backend.Services;

public interface AzuriteService
{
    public Task<string> UploadImageThenGetUrl(byte[] imageBytes, string imageName);
}
