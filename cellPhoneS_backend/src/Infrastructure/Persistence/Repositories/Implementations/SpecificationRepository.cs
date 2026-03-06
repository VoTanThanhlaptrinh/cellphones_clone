using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class SpecificationRepository : BaseRepository<Specification>, ISpecificationRepository
{
    public SpecificationRepository(ApplicationDbContext context) : base(context) {}
}
