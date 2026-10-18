using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

/// <summary>Kho và tồn kho cho trang quản trị: danh sách, đặt tuyệt đối, điều chỉnh nguyên tử.</summary>
public interface IAdminInventoryService
{
    Task<PagedResult<AdminStoreHouseView>> GetStoreHouses(AdminStoreHouseFilter filter);
    Task<PagedResult<InventoryRowView>> GetInventory(InventoryFilter filter);
    /// <summary>Đặt số lượng của (kho, sản phẩm, màu); tạo dòng nếu chưa có.</summary>
    Task SetQuantity(SetInventoryRequest request, string userId);
    /// <summary>Cộng/trừ nguyên tử; trả số lượng mới. Kết quả &lt; 0 -> <see cref="BadRequestException"/>.</summary>
    Task<int> Adjust(AdjustInventoryRequest request, string userId);
}
