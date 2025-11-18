using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<ProductView>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Products.Where(p => !"deleted".Equals(p.Status)).Skip(page * pageSize).Take(pageSize).Select(p => new ProductView(p.Id, p.ImageUrl, p.Name, p.BasePrice, p.SalePrice)).ToListAsync();
    }

    public async Task<Product?> GetProductDetails(long id)
    {
        return await _context.Products.Where(p => !"deleted".Equals(p.Status)).FirstOrDefaultAsync(p => p.Id == id);
    }
}
