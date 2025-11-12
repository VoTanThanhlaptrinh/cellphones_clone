using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class CriterionDetailRepository : BaseRepository<CriterionDetail>, ICriterionDetailRepository
{
    public CriterionDetailRepository(ApplicationDbContext context) : base(context) {}
}
