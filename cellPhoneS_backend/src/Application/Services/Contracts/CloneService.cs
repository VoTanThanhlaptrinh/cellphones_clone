using System;

namespace cellPhoneS_backend.Services;

public interface CloneService
{
    Task<string> GetImageUrlFromOnlineAfterUploadOnAzurite(string url);
}
