using System;

namespace cellPhoneS_backend.Services;

public interface ImageDownloaderService
{
    public Task<byte[]> DownloadImageFromUrl(string url);
}
