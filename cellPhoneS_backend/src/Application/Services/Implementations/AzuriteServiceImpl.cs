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
        // _blobContainerClient.CreateIfNotExists(PublicAccessType.Blob); // optional public access
    }
    public async Task<string> UploadImageThenGetUrl(byte[] imageBytes, string imageName)
    {
        string fullBlobName = $"{_folderPrefix}/{imageName}";
        var blobClient = _blobContainerClient.GetBlobClient(fullBlobName);
        using (var stream = new MemoryStream(imageBytes))
        {
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "image/png" }
            };
            await blobClient.UploadAsync(stream, uploadOptions);
        }
        return blobClient.Uri.ToString();
    }
}
