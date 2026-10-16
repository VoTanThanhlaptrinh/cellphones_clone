using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellphones_backend.Services.Interface;

public interface StoreHouseService
{
    // Admin operations
    Task<long> CreateStoreHouse(CreateStoreHouseRequest request, string userId);
    Task UpdateStoreHouse(long storeHouseId, UpdateStoreHouseRequest request, string userId);
    Task DeleteStoreHouse(long storeHouseId, string userId);
    
    // Public/User operations
    Task<PagedResult<StoreHouseResponse>> GetAllStoreHouses(int pageIndex, int pageSize, string? status = "active");
    Task<StoreHouseResponse> GetStoreHouseById(long storeHouseId);
}
