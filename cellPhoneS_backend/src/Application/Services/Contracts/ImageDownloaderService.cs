using System;

namespace cellPhoneS_backend.Services;

public interface ImageDownloaderService
{
    Task<byte[]> DownloadImageFromUrl(string url);
}
