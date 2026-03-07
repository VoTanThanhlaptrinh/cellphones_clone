using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface IStoreRepository : IRepository<Store>
{
    Task<int> GetTotalQuantityInStoreByProductAndColor(long productCartDetailId, long colorId);
    Task<Store?> FindByProductAndColorAsync(long productCartDetailId, long colorId);
    Task<List<StoreInventoryDTO>> AllocateAllStockAsync(List<CartDetail> cartItems);
    Task<List<Store>> GetStoresAsync(List<OrderDetail> orderDetails);
    public Task UpdateRangeAsync(List<StoreInventoryDTO> modifiedStores);
}
