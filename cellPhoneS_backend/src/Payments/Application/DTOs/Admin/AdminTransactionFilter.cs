using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>
/// Bộ lọc <c>GET /api/admin/transactions</c>. <c>Provider</c>: PAYOS/VNPAY (không phân biệt hoa thường, so với tên hiển thị lưu trong giao dịch);
/// <c>Status</c>: trạng thái giao dịch (hiện chỉ có success); <c>Search</c> khớp mã tham chiếu, link thanh toán hoặc email khách;
/// <c>SortBy</c>: createDate (mặc định), amount.
/// </summary>
public class AdminTransactionFilter : AdminListQuery
{
    public string? Provider { get; set; }
    public long? OrderId { get; set; }
}
