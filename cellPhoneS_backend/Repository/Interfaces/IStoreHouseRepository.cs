using System.Collections.ObjectModel;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface IStoreHouseRepository : IRepository<StoreHouse>
{
    Task<List<StoreView>> GetStoreViews();
}
