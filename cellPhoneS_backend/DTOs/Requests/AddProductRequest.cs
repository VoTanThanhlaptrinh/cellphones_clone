using System;
using System.ComponentModel.DataAnnotations;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;

namespace cellPhoneS_backend.J2O;

public record AddProductRequest
{
    // Ensures the product has a name for display on the storefront and in search results
    // A missing name would make the product unsearchable and unidentifiable to customers
    [Required(ErrorMessage = "Product name is required. / Tên sản phẩm là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Product name must not exceed 255 characters. / Tên sản phẩm không được vượt quá 255 ký tự.")]
    public string? Name { get; set; }

    // Ensures the base price is valid for financial calculations and profit margin analysis
    // Negative prices would break the pricing system and cause revenue calculation errors
    [Required(ErrorMessage = "Base price is required. / Giá gốc là bắt buộc.")]
    [Range(0, double.MaxValue, ErrorMessage = "Base price must be greater than or equal to 0. / Giá gốc phải lớn hơn hoặc bằng 0.")]
    public double BasePrice { get; set; }

    // Ensures the sale price is valid and can be displayed to customers
    // Negative sale prices would confuse customers and break the checkout process
    [Required(ErrorMessage = "Sale price is required. / Giá bán là bắt buộc.")]
    [Range(0, double.MaxValue, ErrorMessage = "Sale price must be greater than or equal to 0. / Giá bán phải lớn hơn hoặc bằng 0.")]
    public double SalePrice { get; set; }

    // Ensures the version string doesn't exceed database column limits
    // Version helps distinguish between product variants (e.g., "iPhone 15 Pro Max 256GB")
    [MaxLength(255, ErrorMessage = "Version must not exceed 255 characters. / Phiên bản không được vượt quá 255 ký tự.")]
    public string? Version { get; set; }

    // Ensures the thumbnail image is provided for product listing pages
    // Without a thumbnail, the product cannot be properly displayed in category or search pages
    [Required(ErrorMessage = "Thumbnail image is required. / Ảnh đại diện là bắt buộc.")]
    public ImageDTO? ThumbnailImage { get; set; }

    // Allows additional product images for the detail page gallery
    // Multiple images help customers see the product from different angles
    public List<ImageDTO>? ProductImages { get; set; }

    // Ensures technical specifications are provided for informed purchase decisions
    // Specifications help customers compare products and understand features
    public List<SpecificationDTO>? Specifications { get; set; }

    // Ensures the product is categorized for proper navigation and filtering
    // Without a category, the product cannot appear in category-based browsing
    [Required(ErrorMessage = "Category ID is required. / Mã danh mục là bắt buộc.")]
    [Range(1, long.MaxValue, ErrorMessage = "Category ID must be greater than 0. / Mã danh mục phải lớn hơn 0.")]
    public long? CategoryId { get; set; }

    // Ensures the product is associated with a brand for filtering and brand pages
    // Brand association helps with customer brand loyalty and filtering features
    [Required(ErrorMessage = "Brand ID is required. / Mã thương hiệu là bắt buộc.")]
    [Range(1, long.MaxValue, ErrorMessage = "Brand ID must be greater than 0. / Mã thương hiệu phải lớn hơn 0.")]
    public long? BrandId { get; set; }

    // Allows optional warranty and service commitments to build customer trust
    // Commitments like "12-month warranty" or "Free shipping" increase conversion rates
    public List<string>? Commitments { get; set; }

    // Ensures color variants are defined for inventory tracking and customer selection
    // Products without colors cannot be added to cart or tracked in inventory
    [Required(ErrorMessage = "At least one color is required. / Ít nhất một màu sắc là bắt buộc.")]
    [MinLength(1, ErrorMessage = "At least one color must be provided. / Phải cung cấp ít nhất một màu sắc.")]
    public List<ColorRequest>? Colors { get; set; }

    // Ensures initial inventory is set up for fulfillment
    // Without inventory data, the product cannot be sold even if it exists in the catalog
    [Required(ErrorMessage = "Initial inventory is required. / Tồn kho ban đầu là bắt buộc.")]
    [MinLength(1, ErrorMessage = "At least one inventory entry must be provided. / Phải cung cấp ít nhất một bản ghi tồn kho.")]
    public List<StoreDTO>? InitialInventory { get; set; }

    // Allows manual status override during product creation
    // Default is "active" but admin may want to create as "draft" for review
    [MaxLength(50, ErrorMessage = "Status must not exceed 50 characters. / Trạng thái không được vượt quá 50 ký tự.")]
    public string? Status { get; set; } = "active";
}