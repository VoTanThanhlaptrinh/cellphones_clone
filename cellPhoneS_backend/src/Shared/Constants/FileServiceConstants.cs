namespace cellPhoneS_backend.Constants;

/// <summary>Khoá cấu hình của file-service (upload ảnh lên R2). API key chỉ nằm ở backend.</summary>
public static class FileServiceConfigKeys
{
    public const string BaseUrl = "FileService:BaseUrl";
    public const string ApiKey = "FileService:ApiKey";
    public const string DefaultBaseUrl = "https://upload-service-gamma.vercel.app/";
}

/// <summary>Context upload của web cellphones tại file-service.</summary>
public static class FileContexts
{
    /// <summary>Ảnh công khai: jpeg, png, webp, tối đa 2 MB.</summary>
    public const string ProductGallery = "product-gallery";

    /// <summary>Tiền tố tempKey hợp lệ cho context này (do file-service tạo ra).</summary>
    public const string ProductGalleryTempPrefix = "temp/public/cellphones/product-gallery/";

    public const long ProductGalleryMaxBytes = 2 * 1024 * 1024;

    public static readonly string[] ProductGalleryMimeTypes = { "image/jpeg", "image/png", "image/webp" };
}
