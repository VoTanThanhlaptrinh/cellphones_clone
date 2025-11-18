using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface IProductRepository : IRepository<Product>
{
    public Task<Product?> GetProductDetails(long id);
    public Task<IEnumerable<ProductView>> GetAllAsync(int page, int pageSize);
}
