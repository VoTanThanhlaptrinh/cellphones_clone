using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class CriterionRepository : BaseRepository<Criterion>, ICriterionRepository
{
    public CriterionRepository(ApplicationDbContext context) : base(context) {}
}
