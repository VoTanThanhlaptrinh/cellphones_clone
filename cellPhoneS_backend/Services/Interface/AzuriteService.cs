using System;

namespace cellPhoneS_backend.Services;

public interface AzuriteService
{
    Task<string> UploadImageThenGetUrl(byte[] imageBytes, string imageName);
}
