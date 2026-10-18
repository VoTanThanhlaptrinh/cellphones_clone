using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellPhoneS_backend.Constants;

namespace cellphones_backend.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // === BEGIN NEW FEATURE: PRODUCT REVIEWS ===
        [Required]
        public long ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>visible (mặc định) hoặc hidden do admin ẩn đi; xem <c>ReviewStatuses</c>.</summary>
        [Required]
        [MaxLength(16)]
        public string Status { get; set; } = ReviewStatuses.Visible;
        // === END NEW FEATURE: PRODUCT REVIEWS ===
    }
}
