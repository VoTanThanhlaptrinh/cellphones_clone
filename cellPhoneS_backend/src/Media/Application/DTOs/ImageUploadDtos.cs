using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public class PresignImageUploadRequest
{
    [Required(ErrorMessage = "Tên file là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Tên file không được vượt quá 255 ký tự.")]
    public string? FileName { get; set; }

    [Required(ErrorMessage = "Loại file là bắt buộc.")]
    [RegularExpression(@"^image\/(jpeg|png|webp)$", ErrorMessage = "Chỉ hỗ trợ ảnh JPG, PNG hoặc WEBP.")]
    public string? MimeType { get; set; }

    [Range(1, 2 * 1024 * 1024, ErrorMessage = "Dung lượng ảnh phải từ 1 byte đến 2MB.")]
    public long Size { get; set; }
}

public record PresignImageUploadResponse(string TempKey, string UploadUrl, int ExpiresIn, string? PreviewUrl);

public class ConfirmImageUploadRequest
{
    [Required(ErrorMessage = "tempKey là bắt buộc.")]
    [MaxLength(500)]
    public string? TempKey { get; set; }

    [Required(ErrorMessage = "Tên ảnh là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Tên ảnh không được vượt quá 255 ký tự.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Loại file là bắt buộc.")]
    [RegularExpression(@"^image\/(jpeg|png|webp)$", ErrorMessage = "Chỉ hỗ trợ ảnh JPG, PNG hoặc WEBP.")]
    public string? MimeType { get; set; }

    [MaxLength(255, ErrorMessage = "Alt text không được vượt quá 255 ký tự.")]
    public string? Alt { get; set; }
}
