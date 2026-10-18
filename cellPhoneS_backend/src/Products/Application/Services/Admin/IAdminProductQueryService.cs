using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Đọc sản phẩm cho trang quản trị (mọi trạng thái, lọc, sắp xếp, phân trang).</summary>
public interface IAdminProductQueryService
{
    Task<PagedResult<AdminProductView>> GetProducts(AdminProductFilter filter);
    Task<AdminProductDetail> GetProduct(long productId);
}
