using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace cellPhoneS_backend.Services.Implement;

public class AzuriteServiceImpl : AzuriteService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly string? _containerName;
    private readonly string _folderPrefix = "p/tablet";
    public AzuriteServiceImpl(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = "images";
        _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        // Allow full access to images 
        // _blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
    }
    // Upload Image into Azurite and get the url after finish
    public async Task<string> UploadImageThenGetUrl(byte[] imageBytes, string imageName)
    {
        string fullBlobName = $"{_folderPrefix}/{imageName}";
        var blobClient = _blobContainerClient.GetBlobClient(fullBlobName);
        using (var stream = new MemoryStream(imageBytes))
        {
            var uploadOptions = new BlobUploadOptions
            {
                // 2. Thiết lập HttpHeaders
                HttpHeaders = new BlobHttpHeaders
                {
                    // 3. Đặt ContentType!
                    // (Chúng ta biết link gốc là file .png, nên ta sẽ đặt là "image/png")
                    ContentType = "image/png"
                },

            };
            await blobClient.UploadAsync(stream, uploadOptions);
        }

        return blobClient.Uri.ToString();
    }
}
