namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Chi tiết một đơn trong trang quản trị (thêm so với dòng danh sách).</summary>
public class AdminOrderDetail : AdminOrderView
{
    public string? CustomerId { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime UpdateDate { get; set; }
    /// <summary>Email người cập nhật gần nhất (khách hoặc admin).</summary>
    public string? UpdatedByEmail { get; set; }
    /// <summary>Số lần thử thanh toán online (0 = chưa thử). Idempotency-Key gửi gateway: cellphones-order-{id}-attempt-{n}.</summary>
    public int PaymentAttempt { get; set; }
    public List<FeeView> Fees { get; set; } = new List<FeeView>();
    /// <summary>Kho xuất hàng (với đơn pickup là cửa hàng nhận). Đơn không lưu địa chỉ giao hàng của khách.</summary>
    public List<AdminOrderStoreHouseView> StoreHouses { get; set; } = new List<AdminOrderStoreHouseView>();
    public List<AdminTransactionView> Transactions { get; set; } = new List<AdminTransactionView>();
}

/// <summary>Một kho xuất hàng của đơn kèm số lượng xuất theo từng sản phẩm/màu.</summary>
public class AdminOrderStoreHouseView
{
    public long Id { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public List<AdminOrderAllocationView> Items { get; set; } = new List<AdminOrderAllocationView>();
}

public record AdminOrderAllocationView(long ProductId, string? ProductName, long ColorId, string? ColorName, int Quantity);

/// <summary>Kết quả huỷ đơn: trạng thái trước khi huỷ và có cần hoàn tiền thủ công hay không (đã có giao dịch thanh toán thành công).</summary>
public record CancelOrderResult(string PreviousStatus, bool RefundRequired);
