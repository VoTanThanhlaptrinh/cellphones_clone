using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class SpecificationDetailRepository : BaseRepository<SpecificationDetail>, ISpecificationDetailRepository
{
    public SpecificationDetailRepository(ApplicationDbContext context) : base(context) {}
}
