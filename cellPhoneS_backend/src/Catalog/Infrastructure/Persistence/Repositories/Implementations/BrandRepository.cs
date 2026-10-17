using cellphones_backend.Data;
using cellphones_backend.Models;

namespace cellphones_backend.Repositories;

public class BrandRepository : BaseRepository<Brand>, IBrandRepository
{
    public BrandRepository(ApplicationDbContext context) : base(context) {}
}
