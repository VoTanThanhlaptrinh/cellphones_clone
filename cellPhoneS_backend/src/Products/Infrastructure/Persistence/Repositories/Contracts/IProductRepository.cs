using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;

namespace cellphones_backend.Repositories;

public interface IProductRepository : IRepository<Product>
{
    public Task<ProductViewDetail?> GetProductDetails(long id);
    public Task<IEnumerable<ProductView>> GetAllAsync(int page, int pageSize);
}
