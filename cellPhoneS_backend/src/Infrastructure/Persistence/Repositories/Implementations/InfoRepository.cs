using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class InfoRepository : BaseRepository<Info>, IInfoRepository
{
    public InfoRepository(ApplicationDbContext context) : base(context) {}
}
