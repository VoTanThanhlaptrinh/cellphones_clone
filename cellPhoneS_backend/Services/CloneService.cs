using System;

namespace cellPhoneS_backend.Services;

public interface CloneService
{
    public Task<string> GetImageUrlFromOnlineAfterUploadOnAzurite(string url);
}
