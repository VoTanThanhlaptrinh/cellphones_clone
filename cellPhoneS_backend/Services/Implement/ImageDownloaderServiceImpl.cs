using System;

namespace cellPhoneS_backend.Services.Implement;

public class ImageDownloaderServiceImpl : ImageDownloaderService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ImageDownloaderServiceImpl(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<byte[]> DownloadImageFromUrl(string url)
    {
        var client = _httpClientFactory.CreateClient();
        return await GetByteFromStream(client.GetStreamAsync(url));
    }

    private async Task<byte[]> GetByteFromStream(Task<Stream> streamTask)
    {
        // đây là hàm đọc Từ Stream về byte[]
        using (var memorySt = new MemoryStream())
        {
            using (Stream stream = await streamTask)
            {
                await stream.CopyToAsync(memorySt);
            }
            return memorySt.ToArray();
        }
    }
}
