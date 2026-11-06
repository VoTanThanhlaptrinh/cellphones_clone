using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;

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
        if (string.IsNullOrWhiteSpace(request.OriginUrl))
            throw new BadRequestException("URL ảnh không được để trống.");

        byte[] bytes = Array.Empty<byte>();
        try
        {
            bytes = await _imageDownloaderService.DownloadImageFromUrl(request.OriginUrl);
        }
        catch
        {
            // If download fails or not reachable, fallback to empty
        }

        string blobUrl = request.OriginUrl;
        if (bytes.Length > 0)
        {
            try
            {
                var fileName = $"{Guid.NewGuid()}_{request.Name ?? "image"}";
                blobUrl = await _azuriteService.UploadImageThenGetUrl(bytes, fileName);
            }
            catch
            {
                blobUrl = request.OriginUrl;
            }
        }

        var image = new Image
        {
            OriginUrl = request.OriginUrl,
            BlobUrl = blobUrl,
            MimeType = request.MimeType ?? "image/jpeg",
            Name = request.Name ?? "image",
            Alt = request.Alt ?? request.Name ?? "image",
            Status = "in-use",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CreateBy = userId,
            UpdateBy = userId
        };
        await _imageRepository.AddAsync(image);
        await _imageRepository.SaveChangesAsync();
        return image.Id;
    }

    public async Task<long> UploadImage(IFormFile file, string? alt, string userId)
    {
        if (file == null || file.Length == 0)
            throw new BadRequestException("File ảnh không hợp lệ hoặc để trống.");

        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        string blobUrl;
        try
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            blobUrl = await _azuriteService.UploadImageThenGetUrl(bytes, fileName);
        }
        catch
        {
            // Fallback: data URI if Azurite storage service is offline
            blobUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
        }

        var image = new Image
        {
            OriginUrl = blobUrl,
            BlobUrl = blobUrl,
            MimeType = file.ContentType,
            Name = file.FileName,
            Alt = alt ?? file.FileName,
            Status = "in-use",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CreateBy = userId,
            UpdateBy = userId
        };
        await _imageRepository.AddAsync(image);
        await _imageRepository.SaveChangesAsync();
        return image.Id;
    }

    public async Task DeleteImage(long imageId, string userId)
    {
        var image = await _imageRepository.GetByIdAsync(imageId);
        if (image == null || image.Status == "deleted")
            throw new NotFoundException($"Không tìm thấy ảnh #{imageId}.");

        image.Status = "deleted";
        image.UpdateDate = DateTime.UtcNow;
        image.UpdateBy = userId;
        await _imageRepository.UpdateAsync(image);
        await _imageRepository.SaveChangesAsync();
    }

    public async Task<ImageDTO> GetImage(long imageId)
    {
        var image = await _imageRepository.GetByIdAsync(imageId);
        if (image == null || image.Status == "deleted")
            throw new NotFoundException($"Không tìm thấy ảnh #{imageId}.");

        return new ImageDTO(image.OriginUrl, image.MimeType, image.Name, image.Alt, image.BlobUrl, image.Id);
    }

    public async Task<List<ImageDTO>> GetAllImages()
    {
        var images = await _imageRepository.FindAsync(i => i.Status != "deleted");
        return images.OrderByDescending(i => i.CreateDate)
            .Select(i => new ImageDTO(i.OriginUrl, i.MimeType, i.Name, i.Alt, i.BlobUrl, i.Id))
            .ToList();
    }
}
