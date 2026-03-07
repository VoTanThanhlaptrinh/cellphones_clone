namespace cellPhoneS_backend.DTOs.Responses;

public record StoreView(string? City, List<DistrictView>? Districts);

public record DistrictView(string? District, List<StreetView>? Streets);

public record StreetView(long Id, string? Street);

public record StoreHouseView(long Id, string? City, string? District, string? Street);

public class StoreInventoryDTO
{
    // Thông tin định danh & số lượng
    public long StoreHouseId { get; set; }
    public long ProductId { get; set; }
    public long ColorId { get; set; }
    public int Quantity { get; set; }

    // Thông tin từ StoreHouse (Để tính phí ship)
    public string? City { get; set; }
    public string? District { get; set; }

    // Thông tin từ Product & Color (Để tạo OrderDetail không bị Null)
    public string? ProductName { get; set; }
    public double SalePrice { get; set; }
    public string? ColorName { get; set; }
    public string? ProductImageUrl { get; set; }
    public string? ColorImageUrl { get; set; }
}