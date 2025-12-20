using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface IStoreRepository : IRepository<Store>
{
    Task<int> GetTotalQuantityInStoreByProductAndColor(long productCartDetailId, long colorId);
}
