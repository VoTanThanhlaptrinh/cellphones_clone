using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Các truy vấn chỉ-đọc (có phân trang và lọc) cho trang quản trị.</summary>
public interface IAdminQueryService
{
    Task<PagedResult<AdminUserView>> GetUsers(int pageIndex, int pageSize, string? search);
    Task<PagedResult<AdminOrderView>> GetOrders(int pageIndex, int pageSize, string? status);
    Task<AdminOrderView> GetOrder(long orderId);
    Task<PagedResult<AdminTransactionView>> GetTransactions(int pageIndex, int pageSize);
}
