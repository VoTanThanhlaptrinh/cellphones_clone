using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public class ColorDTO
{
    // Ensures the color has a valid identifier if it's an existing color being referenced
    // This allows linking products to pre-existing color definitions in the database
    public long? Id { get; set; }

    // Ensures the color name is provided as it's displayed to customers during product selection
    // A missing color name would confuse users about which variant they're choosing
    [Required(ErrorMessage = "Color name is required. / Tên màu sắc là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Color name must not exceed 100 characters. / Tên màu sắc không được vượt quá 100 ký tự.")]
    public string? Name { get; set; }

    // Ensures the color code is valid for UI rendering purposes
    // Invalid hex codes would break the visual color display on the product page
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", 
        ErrorMessage = "Color code must be a valid hex color (e.g., #FF5733). / Mã màu phải là mã hex hợp lệ (ví dụ: #FF5733).")]
    public string? ColorCode { get; set; }
}
