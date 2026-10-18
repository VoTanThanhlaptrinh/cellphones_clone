using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Danh sách giao dịch thanh toán (chỉ đọc) cho trang quản trị (module Payments).</summary>
public interface IAdminTransactionService
{
    Task<PagedResult<AdminTransactionView>> GetTransactions(AdminTransactionFilter filter);
}
