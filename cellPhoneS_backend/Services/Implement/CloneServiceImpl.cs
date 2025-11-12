using System;

namespace cellPhoneS_backend.Services.Implement;

public class CloneServiceImpl : CloneService
{
    private readonly ImageDownloaderService _imageDownloaderService;
    private readonly AzuriteService _azuriteService;
    public CloneServiceImpl(ImageDownloaderService imageDownloaderService, AzuriteService azuriteService)
    {
        _imageDownloaderService = imageDownloaderService;
        _azuriteService = azuriteService;
    }
    public async Task<string> GetImageUrlFromOnlineAfterUploadOnAzurite(string url)
    {

        return await _azuriteService.UploadImageThenGetUrl(await _imageDownloaderService.DownloadImageFromUrl(url), Guid.NewGuid().ToString() + ".png");
    }
}
