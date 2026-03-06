using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class GrantRoleRepository : BaseRepository<GrantRole>, IGrantRoleRepository
{
    public GrantRoleRepository(ApplicationDbContext context) : base(context) {}
}
