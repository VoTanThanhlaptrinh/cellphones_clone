using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public class CreateSpecificationRequest
{
    // Ensures the specification has a name for categorization and display
    // Specification names like "Display", "Camera", "Battery" help organize product details
    [Required(ErrorMessage = "Specification name is required. / Tên thông số là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Specification name must not exceed 255 characters. / Tên thông số không được vượt quá 255 ký tự.")]
    public string? Name { get; set; }

    // Allows initial specification details to be created along with the specification
    // Details like "Screen Size: 6.7 inches" provide the actual product information
    public List<CreateSpecificationDetailRequest>? Details { get; set; }
}

public class CreateSpecificationDetailRequest
{
    // Ensures the detail has a descriptive name for the attribute being specified
    // Names like "Screen Size", "Resolution", "RAM" identify what's being measured
    [Required(ErrorMessage = "Specification detail name is required. / Tên chi tiết thông số là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Specification detail name must not exceed 255 characters. / Tên chi tiết thông số không được vượt quá 255 ký tự.")]
    public string? Name { get; set; }

    // Ensures the detail has a value that describes the product attribute
    // Values like "6.7 inches", "1080x2400", "8GB" provide the actual specification
    [Required(ErrorMessage = "Specification detail value is required. / Giá trị chi tiết thông số là bắt buộc.")]
    [MaxLength(1000, ErrorMessage = "Specification detail value must not exceed 1000 characters. / Giá trị chi tiết thông số không được vượt quá 1000 ký tự.")]
    public string? Value { get; set; }
}
