using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class CategoryProductRepository : BaseRepository<CategoryProduct>, ICategoryProductRepository
{
    public CategoryProductRepository(ApplicationDbContext context) : base(context) {}
}
