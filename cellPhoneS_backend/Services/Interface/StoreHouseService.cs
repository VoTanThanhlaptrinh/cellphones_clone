using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services.Interface;

public interface StoreHouseService
{
    // Admin operations
    Task<ServiceResult<long>> CreateStoreHouse(CreateStoreHouseRequest request, string userId);
    Task<ServiceResult<string>> UpdateStoreHouse(long storeHouseId, UpdateStoreHouseRequest request, string userId);
    Task<ServiceResult<string>> DeleteStoreHouse(long storeHouseId, string userId);
    
    // Public/User operations
    Task<ServiceResult<PagedResult<StoreHouseResponse>>> GetAllStoreHouses(int pageIndex, int pageSize, string? status = "active");
    Task<ServiceResult<StoreHouseResponse>> GetStoreHouseById(long storeHouseId);
}
