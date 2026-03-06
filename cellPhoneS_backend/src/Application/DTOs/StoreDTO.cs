using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs;

public class StoreDTO
{
    // Ensures the warehouse location is specified for inventory tracking
    // Without a storehouse ID, we cannot track where the product stock is physically located
    [Required(ErrorMessage = "Storehouse ID is required. / Mã kho hàng là bắt buộc.")]
    [Range(1, long.MaxValue, ErrorMessage = "Storehouse ID must be greater than 0. / Mã kho hàng phải lớn hơn 0.")]
    public long StoreHouseId { get; set; }

    // Ensures the color variant is specified for this inventory entry
    // Multi-color products need separate inventory tracking per color
    [Required(ErrorMessage = "Color ID is required. / Mã màu sắc là bắt buộc.")]
    [Range(1, long.MaxValue, ErrorMessage = "Color ID must be greater than 0. / Mã màu sắc phải lớn hơn 0.")]
    public long ColorId { get; set; }

    // Ensures the initial stock quantity is non-negative
    // Negative inventory would indicate a data entry error and cause fulfillment issues
    [Required(ErrorMessage = "Quantity is required. / Số lượng là bắt buộc.")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0. / Số lượng phải lớn hơn hoặc bằng 0.")]
    public int Quantity { get; set; }
}
