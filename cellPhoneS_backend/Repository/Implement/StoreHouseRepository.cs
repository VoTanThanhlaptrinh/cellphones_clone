using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class StoreHouseRepository : BaseRepository<StoreHouse>, IStoreHouseRepository
{
    public StoreHouseRepository(ApplicationDbContext context) : base(context) {}
}
