using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface IStoreRepository : IRepository<Store>
{
    Task<int> GetTotalQuantityInStoreByProductAndColor(long productCartDetailId, long colorId);
    Task<Store?> FindByProductAndColorAsync(long productCartDetailId, long colorId);
    Task<List<Store>> AllocateAllStockAsync(List<CartDetail> cartItems, string customerProvince);
    Task<List<Store>> GetInventoryForStoreAsync(List<CartDetail> cartItems, long storeHouseId);
}
