using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public class CreateCommitmentRequest
{
    // Ensures the commitment is associated with a specific product
    // Without a product ID, we cannot display the commitment on the correct product page
    [Required(ErrorMessage = "Product ID is required. / Mã sản phẩm là bắt buộc.")]
    [Range(1, long.MaxValue, ErrorMessage = "Product ID must be greater than 0. / Mã sản phẩm phải lớn hơn 0.")]
    public long ProductId { get; set; }

    // Ensures the commitment has descriptive text to display to customers
    // Empty commitments would confuse customers about what guarantees they receive
    [Required(ErrorMessage = "Commitment context is required. / Nội dung cam kết là bắt buộc.")]
    [MaxLength(500, ErrorMessage = "Commitment context must not exceed 500 characters. / Nội dung cam kết không được vượt quá 500 ký tự.")]
    [MinLength(10, ErrorMessage = "Commitment context must be at least 10 characters. / Nội dung cam kết phải có ít nhất 10 ký tự.")]
    public string? Context { get; set; }
}

public class CreateMultipleCommitmentsRequest
{
    // Ensures commitments are linked to the correct product
    // Batch creation requires product association for all commitments
    [Required(ErrorMessage = "Product ID is required. / Mã sản phẩm là bắt buộc.")]
    [Range(1, long.MaxValue, ErrorMessage = "Product ID must be greater than 0. / Mã sản phẩm phải lớn hơn 0.")]
    public long ProductId { get; set; }

    // Ensures at least one commitment is provided when using batch creation
    // Empty lists would waste API calls and database transactions
    [Required(ErrorMessage = "Commitment list is required. / Danh sách cam kết là bắt buộc.")]
    [MinLength(1, ErrorMessage = "At least one commitment must be provided. / Phải cung cấp ít nhất một cam kết.")]
    public List<string>? Commitments { get; set; }
}
