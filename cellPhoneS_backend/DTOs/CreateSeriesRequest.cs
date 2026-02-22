using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public class CreateSeriesRequest
{
    // Ensures the series is associated with a brand for proper categorization
    // Series like "iPhone 15", "Galaxy S24" must belong to brands like "Apple", "Samsung"
    [Required(ErrorMessage = "Brand ID is required. / Mã thương hiệu là bắt buộc.")]
    [Range(1, long.MaxValue, ErrorMessage = "Brand ID must be greater than 0. / Mã thương hiệu phải lớn hơn 0.")]
    public long BrandId { get; set; }

    // Ensures the series has a name for display and filtering purposes
    // Missing names would make it impossible to organize products by series
    [Required(ErrorMessage = "Series name is required. / Tên dòng sản phẩm là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Series name must not exceed 255 characters. / Tên dòng sản phẩm không được vượt quá 255 ký tự.")]
    public string? Name { get; set; }

    // Allows optional status override during series creation
    // Default is "active" but admin may want to create as "draft" for future release
    [MaxLength(50, ErrorMessage = "Status must not exceed 50 characters. / Trạng thái không được vượt quá 50 ký tự.")]
    public string? Status { get; set; } = "active";
}
