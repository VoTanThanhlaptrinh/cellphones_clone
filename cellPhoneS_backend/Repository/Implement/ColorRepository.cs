using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class ColorRepository : BaseRepository<Color>, IColorRepository
{
    public ColorRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Color> FindById(long colorId)
    {
        return await _context.Colors
        .SingleOrDefaultAsync(c => c.Id == colorId);
    }
}
