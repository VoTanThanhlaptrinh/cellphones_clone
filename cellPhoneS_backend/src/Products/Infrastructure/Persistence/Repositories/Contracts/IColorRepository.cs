using cellphones_backend.Models;

namespace cellphones_backend.Repositories;

public interface IColorRepository : IRepository<Color>
{
    Task<Color> FindById(long colorId);
}
