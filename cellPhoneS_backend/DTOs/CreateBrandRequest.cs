using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public class CreateBrandRequest
{
    // Ensures the brand is categorized under the correct product category
    // Brands like "Apple", "Samsung" belong to categories like "Smartphones"
    [Required(ErrorMessage = "Category ID is required. / Mã danh mục là bắt buộc.")]
    [Range(1, long.MaxValue, ErrorMessage = "Category ID must be greater than 0. / Mã danh mục phải lớn hơn 0.")]
    public long CategoryId { get; set; }

    // Ensures the brand has a recognizable name for customer filtering and search
    // Missing brand names would break brand-based navigation and filtering
    [Required(ErrorMessage = "Brand name is required. / Tên thương hiệu là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Brand name must not exceed 255 characters. / Tên thương hiệu không được vượt quá 255 ký tự.")]
    public string? Name { get; set; }

    // Allows optional brand logo image for visual brand identification
    // Brand logos help customers quickly identify manufacturers on product listings
    [Range(1, long.MaxValue, ErrorMessage = "Image ID must be greater than 0 if provided. / Mã ảnh phải lớn hơn 0 nếu được cung cấp.")]
    public long? ImageId { get; set; }

    // Allows optional status override during brand creation
    // Default is "active" but admin may want to create as "draft" before publishing
    [MaxLength(50, ErrorMessage = "Status must not exceed 50 characters. / Trạng thái không được vượt quá 50 ký tự.")]
    public string? Status { get; set; } = "active";
}
