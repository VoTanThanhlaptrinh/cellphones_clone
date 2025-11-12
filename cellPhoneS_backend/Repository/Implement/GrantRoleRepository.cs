using cellphones_backend.Data;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class GrantRoleRepository : BaseRepository<GrantRole>, IGrantRoleRepository
{
    public GrantRoleRepository(ApplicationDbContext context) : base(context) {}
}
