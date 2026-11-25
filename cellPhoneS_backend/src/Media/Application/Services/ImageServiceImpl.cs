using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;
using cellPhoneS_backend.Services.Interface;

namespace cellphones_backend.Services.Implement;

public class ImageServiceImpl : ImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly IFileServiceClient _fileService;
    private readonly ImageDownloaderService _imageDownloaderService;
    private readonly ILogger<ImageServiceImpl> _logger;

    public ImageServiceImpl(
        IImageRepository imageRepository,
        IFileServiceClient fileService,
        ImageDownloaderService imageDownloaderService,
        ILogger<ImageServiceImpl> logger)
    {
        _imageRepository = imageRepository;
        _fileService = fileService;
        _imageDownloaderService = imageDownloaderService;
        _logger = logger;
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không tải được ảnh từ {Url}, giữ link gốc", request.OriginUrl);
        }

        // Lưu bản sao lên R2 qua file-service; lỗi hoặc định dạng/dung lượng không được nhận thì dùng lại link gốc.
        string blobUrl = request.OriginUrl;
        string? storageKey = null;
        var mimeType = request.MimeType ?? "image/jpeg";
        if (bytes.Length > 0)
        {
            if (!FileContexts.ProductGalleryMimeTypes.Contains(mimeType) || bytes.Length > FileContexts.ProductGalleryMaxBytes)
            {
                _logger.LogInformation("Ảnh {Url} ({Mime}, {Size} byte) không đủ điều kiện lưu lên R2, giữ link gốc", request.OriginUrl, mimeType, bytes.Length);
            }
            else
            {
                try
                {
                    var fileName = $"{request.Name ?? "image"}{ExtensionOf(mimeType)}";
                    var presign = await _fileService.PresignAsync(FileContexts.ProductGallery, fileName, mimeType, bytes.Length);
                    await _fileService.UploadToPresignedUrlAsync(presign.UploadUrl, bytes, mimeType);
                    var confirmed = await _fileService.ConfirmAsync(presign.TempKey);
                    if (!string.IsNullOrEmpty(confirmed.Url))
                    {
                        blobUrl = confirmed.Url;
                        storageKey = confirmed.Key;
                    }
                }
                catch (AppException ex)
                {
                    _logger.LogWarning(ex, "Không lưu được ảnh {Url} lên file-service, giữ link gốc", request.OriginUrl);
                }
            }
        }

        var image = new Image
        {
            OriginUrl = request.OriginUrl,
            BlobUrl = blobUrl,
            StorageKey = storageKey,
            MimeType = mimeType,
            Name = request.Name ?? "image",
            Alt = request.Alt ?? request.Name ?? "image",
            Status = "in-use",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CreateBy = userId,
            UpdateBy = userId
        };
        await SaveNewImage(image);
        return image.Id;
    }

    public async Task<PresignImageUploadResponse> PresignUpload(PresignImageUploadRequest request)
    {
        var result = await _fileService.PresignAsync(
            FileContexts.ProductGallery, request.FileName!, request.MimeType!, request.Size);
        return new PresignImageUploadResponse(result.TempKey, result.UploadUrl, result.ExpiresIn, result.PreviewUrl);
    }

    public async Task<long> ConfirmUpload(ConfirmImageUploadRequest request, string userId)
    {
        var tempKey = request.TempKey!;
        if (!tempKey.StartsWith(FileContexts.ProductGalleryTempPrefix, StringComparison.Ordinal) || tempKey.Contains(".."))
            throw new BadRequestException("tempKey không hợp lệ.");

        var confirmed = await _fileService.ConfirmAsync(tempKey);
        if (string.IsNullOrEmpty(confirmed.Url))
            throw new InternalServerException("Dịch vụ lưu trữ không trả về đường dẫn ảnh.");

        var image = new Image
        {
            OriginUrl = confirmed.Url,
            BlobUrl = confirmed.Url,
            StorageKey = confirmed.Key,
            MimeType = request.MimeType,
            Name = request.Name,
            Alt = string.IsNullOrWhiteSpace(request.Alt) ? request.Name : request.Alt,
            Status = "in-use",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CreateBy = userId,
            UpdateBy = userId
        };

        try
        {
            await SaveNewImage(image);
        }
        catch
        {
            // Không để file mồ côi trên R2 khi lưu DB thất bại.
            await TryDeleteFile(confirmed.Key);
            throw;
        }
        return image.Id;
    }

    public async Task DeleteImage(long imageId, string userId)
    {
        var image = await _imageRepository.GetByIdAsync(imageId);
        if (image == null || image.Status == "deleted")
            throw new NotFoundException($"Không tìm thấy ảnh #{imageId}.");

        // Ảnh còn gắn sản phẩm/nhu cầu thì chỉ ẩn khỏi thư viện, giữ file để trang đang dùng không vỡ ảnh.
        if (!string.IsNullOrEmpty(image.StorageKey) && !await _imageRepository.IsReferencedAsync(imageId))
        {
            if (await TryDeleteFile(image.StorageKey))
                image.StorageKey = null;
        }

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

    private async Task SaveNewImage(Image image)
    {
        await _imageRepository.AddAsync(image);
        await _imageRepository.SaveChangesAsync();
    }

    private async Task<bool> TryDeleteFile(string key)
    {
        try
        {
            await _fileService.DeleteAsync(key);
            return true;
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Không xoá được file {Key} trên file-service", key);
            return false;
        }
    }

    private static string ExtensionOf(string mimeType) => mimeType switch
    {
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => ".jpg"
    };
}
