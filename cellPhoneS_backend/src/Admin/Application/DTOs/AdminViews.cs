namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Dòng sản phẩm trong trang quản trị (kèm tổng tồn kho và trạng thái).</summary>
public record AdminProductView(
    long Id,
    string? Name,
    string? ImageUrl,
    double BasePrice,
    double SalePrice,
    string? Status,
    int Stock,
    DateTime CreateDate);

public record AdminUserView(
    string Id,
    string? Email,
    string? FullName,
    string? Phone,
    string Status,
    List<string> Roles,
    DateTime CreateDate);

/// <summary>Đơn hàng trong trang quản trị. `Items` chỉ có khi xem chi tiết một đơn.</summary>
public class AdminOrderView
{
    public long Id { get; set; }
    public DateTime CreateDate { get; set; }
    public string Status { get; set; } = "";
    /// <summary>pickup hoặc delivery.</summary>
    public string Type { get; set; } = "";
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public int ItemCount { get; set; }
    public long ShippingFee { get; set; }
    /// <summary>Tiền hàng cộng phí vận chuyển.</summary>
    public double TotalAmount { get; set; }
    public List<OrderHistoryItemView> Items { get; set; } = new List<OrderHistoryItemView>();
}

public record AdminTransactionView(
    long Id,
    long OrderId,
    long Amount,
    string Provider,
    string? Reference,
    string Status,
    DateTime? TransactionDate,
    DateTime CreateDate,
    string? CustomerEmail);
