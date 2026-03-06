using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class Oauth2Repository : BaseRepository<Oauth2>, IOauth2Repository
{
    public Oauth2Repository(ApplicationDbContext context) : base(context) {}
}
