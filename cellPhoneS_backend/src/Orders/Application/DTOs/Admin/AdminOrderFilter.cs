using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

/// <summary>
/// Bộ lọc <c>GET /api/admin/orders</c>. <c>Status</c>: Pending/Paid/Shipping/Completed/Cancelled; <c>Type</c>: pickup/delivery;
/// <c>Search</c> khớp mã đơn (số), email hoặc tên khách; <c>SortBy</c>: createDate (mặc định), id, status, totalAmount.
/// </summary>
public class AdminOrderFilter : AdminListQuery
{
    public string? Type { get; set; }
}
