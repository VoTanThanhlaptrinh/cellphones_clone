using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class DemandRepository : BaseRepository<Demand>, IDemandRepository
{
    public DemandRepository(ApplicationDbContext context) : base(context) {}
}
