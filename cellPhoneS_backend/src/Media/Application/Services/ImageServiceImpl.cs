using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Implement;

public class ImageServiceImpl : ImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly AzuriteService _azuriteService;
    private readonly ImageDownloaderService _imageDownloaderService;

    public ImageServiceImpl(
        IImageRepository imageRepository, 
        AzuriteService azuriteService,
        ImageDownloaderService imageDownloaderService)
    {
        _imageRepository = imageRepository;
        _azuriteService = azuriteService;
        _imageDownloaderService = imageDownloaderService;
    }

    public async Task<long> CreateImage(CreateImageRequest request, string userId)
    {
        // TODO: Implement business logic using the request and userId here
        // 1. Download image from OriginUrl using _imageDownloaderService.DownloadImageFromUrl()
        // 2. Generate unique filename (e.g., Guid.NewGuid() + extension)
        // 3. Upload to Azure Blob Storage using _azuriteService.UploadImageThenGetUrl()
        // 4. Create Image entity
        // 5. Set OriginUrl, BlobUrl (from Azure), MimeType, Name, Alt from request
        // 6. Set Status = "in-use"
        // 7. Set CreateDate = UpdateDate = DateTime.UtcNow
        // 8. Set CreateBy = UpdateBy = userId
        // 7. Save to database using _imageRepository.AddAsync()
        // 8. Return new Image ID
        
        throw new NotImplementedException();
    }

    public async Task<long> UploadImage(IFormFile file, string? alt, string userId)
    {
        // TODO: Implement business logic using the file, alt, and userId here
        // 1. Validate file size and MIME type
        // 2. Read file bytes into memory
        // 3. Generate unique file name (e.g., Guid.NewGuid() + extension)
        // 4. Upload to Azure Blob Storage using _azuriteService.UploadImageThenGetUrl()
        // 5. Create Image entity
        // 6. Set BlobUrl, OriginUrl (same as BlobUrl), MimeType, Name, Alt
        // 7. Set Status = "in-use"
        // 8. Set CreateDate = UpdateDate = DateTime.UtcNow
        // 9. Set CreateBy = UpdateBy = userId
        // 9. Save to database using _imageRepository.AddAsync()
        // 10. Return new Image ID
        
        throw new NotImplementedException();
    }

    public async Task DeleteImage(long imageId, string userId)
    {
        // TODO: Implement business logic using the imageId and userId here
        // 1. Find image by ID using _imageRepository.GetByIdAsync()
        // 2. If not found, throw NotFoundException
        // 3. Check if image is in use (referenced by products, brands, colors, etc.)
        //    You may need to add a method to check references across multiple tables
        // 4. If in use, throw BadRequestException
        // 5. Set Status = "deleted"
        // 6. Optionally delete from blob storage (or keep for audit purposes)
        // 7. Update UpdateDate = DateTime.UtcNow
        // 8. Update UpdateBy = userId
        // 8. Save changes using _imageRepository.UpdateAsync()
        // 9. Return
        
        throw new NotImplementedException();
    }

    public async Task<ImageDTO> GetImage(long imageId)
    {
        // TODO: Implement business logic here
        // 1. Find image by ID using _imageRepository.GetByIdAsync()
        // 2. If not found or Status = "deleted", throw NotFoundException
        // 3. Project to ImageDTO
        // 4. Return
        
        throw new NotImplementedException();
    }
}
