using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;

namespace cellphones_backend.Repositories;

public interface IStoreHouseRepository : IRepository<StoreHouse>
{
    Task<List<StoreView>> GetStoreViewsAsync();
}
