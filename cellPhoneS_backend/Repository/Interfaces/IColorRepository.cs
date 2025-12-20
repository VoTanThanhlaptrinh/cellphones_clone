using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface IColorRepository : IRepository<Color>
{
    Task<Color> FindById(long colorId);
}
