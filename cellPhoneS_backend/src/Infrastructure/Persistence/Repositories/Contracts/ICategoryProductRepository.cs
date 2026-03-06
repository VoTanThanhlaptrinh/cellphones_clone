using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface ICategoryProductRepository : IRepository<CategoryProduct>
{
    public Task<IEnumerable<ProductView>> GetProductsByCategoryId(long categoryId, int page, int pageSize);
    public Task<IEnumerable<ProductView>> GetProductsBySeriesId(long seriesId, int page, int pageSize);
    public Task<IEnumerable<ProductView>> GetProductsByBrandId(long brandId, int page, int pageSize);
    public Task<IEnumerable<ProductView>> GetProductsByCategorySlugNameForInfinityScroll(string slugName, long? cursor,int size);
}
