using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Các truy vấn chỉ-đọc (có phân trang và lọc) cho trang quản trị.</summary>
public interface IAdminQueryService
{
    Task<PagedResult<AdminProductView>> GetProducts(int pageIndex, int pageSize, string? search);
}
