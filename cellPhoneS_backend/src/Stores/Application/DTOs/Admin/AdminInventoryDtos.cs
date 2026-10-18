using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>
/// <c>GET /api/admin/storehouses</c>. <c>Province</c> khớp (chứa, không phân biệt hoa thường) tỉnh/thành phố của kho; <c>Search</c> khớp địa chỉ.
/// <c>Status</c> bỏ trống = active + inactive, <c>deleted</c> = thùng rác. <c>SortBy</c>: createDate (mặc định), updateDate, city, totalStock, id.
/// </summary>
public class AdminStoreHouseFilter : AdminListQuery
{
    public string? Province { get; set; }
}

/// <summary>
/// <c>GET /api/admin/inventory</c>: mỗi dòng là một tổ hợp (kho, sản phẩm, màu). <c>LowStock</c> là ngưỡng: chỉ lấy dòng có số lượng &lt;= giá trị này.
/// <c>Search</c> khớp tên sản phẩm. <c>SortBy</c>: updateDate (mặc định), createDate, quantity, productName. Dòng của sản phẩm/kho đã xoá bị ẩn.
/// </summary>
public class InventoryFilter : AdminListQuery
{
    public long? ProductId { get; set; }
    public long? StoreHouseId { get; set; }
    public long? ColorId { get; set; }
    public int? LowStock { get; set; }
}

/// <summary>Đặt tuyệt đối số lượng tồn (tạo dòng nếu chưa có). Dễ ghi đè đơn đặt hàng đang chạy: sửa tồn đang bán nên dùng adjust.</summary>
public record SetInventoryRequest(long StoreHouseId, long ProductId, long ColorId, int Quantity);

/// <summary>Cộng (<c>Delta</c> &gt; 0) hoặc trừ (&lt; 0) tồn nguyên tử; kết quả âm bị từ chối. <c>Reason</c> được lưu qua audit log.</summary>
public record AdjustInventoryRequest(long StoreHouseId, long ProductId, long ColorId, int Delta, string? Reason);
