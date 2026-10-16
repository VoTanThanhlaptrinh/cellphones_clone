namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Một dòng hàng trong lịch sử đơn (đã gộp các phần xuất từ nhiều kho cho cùng sản phẩm và màu).</summary>
public record OrderHistoryItemView(
    long ProductId,
    string? ProductName,
    string? ImageUrl,
    long? ColorId,
    string? ColorName,
    int Quantity,
    double Price);

/// <summary>Đơn hàng trong lịch sử mua hàng của người dùng.</summary>
public class OrderHistoryView
{
    public long Id { get; set; }
    public DateTime CreateDate { get; set; }
    /// <summary>Pending, Paid, Shipping, Completed hoặc Cancelled.</summary>
    public string Status { get; set; } = "";
    /// <summary>pickup (nhận tại cửa hàng) hoặc delivery (giao hàng tận nơi).</summary>
    public string Type { get; set; } = "";
    public long ShippingFee { get; set; }
    /// <summary>Tiền hàng cộng phí vận chuyển.</summary>
    public double TotalAmount { get; set; }
    public List<OrderHistoryItemView> Items { get; set; } = new List<OrderHistoryItemView>();
}
