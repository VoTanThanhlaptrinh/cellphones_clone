using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class CommitmentRepository : BaseRepository<Commitment>, ICommitmentRepository
{
    public CommitmentRepository(ApplicationDbContext context) : base(context) {}
}
