using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public class CreateImageRequest
{
    // Ensures the original image URL is provided for download and processing
    // Without the origin URL, we cannot retrieve the image file to upload to blob storage
    [Required(ErrorMessage = "Image origin URL is required. / URL gốc của ảnh là bắt buộc.")]
    [MaxLength(1000, ErrorMessage = "Image origin URL must not exceed 1000 characters. / URL gốc của ảnh không được vượt quá 1000 ký tự.")]
    [Url(ErrorMessage = "Image origin URL must be a valid URL. / URL gốc của ảnh phải là URL hợp lệ.")]
    public string? OriginUrl { get; set; }

    // Ensures the MIME type is specified for proper content-type handling
    // Invalid MIME types would cause browser rendering issues or download problems
    [Required(ErrorMessage = "MIME type is required. / Loại MIME là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "MIME type must not exceed 100 characters. / Loại MIME không được vượt quá 100 ký tự.")]
    [RegularExpression(@"^image\/(jpeg|jpg|png|gif|webp|svg\+xml|bmp)$", 
        ErrorMessage = "MIME type must be a valid image type. / Loại MIME phải là loại ảnh hợp lệ.")]
    public string? MimeType { get; set; }

    // Ensures the image has a descriptive name for file organization and tracking
    // Missing names make it difficult to identify images in blob storage
    [Required(ErrorMessage = "Image name is required. / Tên ảnh là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Image name must not exceed 255 characters. / Tên ảnh không được vượt quá 255 ký tự.")]
    public string? Name { get; set; }

    // Ensures alt text is provided for accessibility and SEO purposes
    // Missing alt text violates WCAG guidelines and reduces search engine visibility
    [Required(ErrorMessage = "Alt text is required for accessibility. / Văn bản thay thế là bắt buộc cho khả năng truy cập.")]
    [MaxLength(255, ErrorMessage = "Alt text must not exceed 255 characters. / Văn bản thay thế không được vượt quá 255 ký tự.")]
    public string? Alt { get; set; }
}
