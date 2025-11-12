using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class ColorRepository : BaseRepository<Color>, IColorRepository
{
    public ColorRepository(ApplicationDbContext context) : base(context) {}
}
