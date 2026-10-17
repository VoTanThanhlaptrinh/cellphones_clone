using cellphones_backend.Data;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class ProductImageRepository : BaseRepository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(ApplicationDbContext context) : base(context) {}
}
