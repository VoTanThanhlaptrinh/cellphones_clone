using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class ProductSpecificationRepository : BaseRepository<ProductSpecification>, IProductSpecificationRepository
{
    public ProductSpecificationRepository(ApplicationDbContext context) : base(context) {}
}
