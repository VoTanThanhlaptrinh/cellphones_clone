using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class SeriesRepository : BaseRepository<Series>, ISeriesRepository
{
    public SeriesRepository(ApplicationDbContext context) : base(context) {}
}
