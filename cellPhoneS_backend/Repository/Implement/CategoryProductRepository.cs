using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class CategoryProductRepository : BaseRepository<CategoryProduct>, ICategoryProductRepository
{
    public CategoryProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<ProductView>> GetProductsByBrandId(long brandId, int page, int pageSize)
    {
        return await _context.CategoryProducts
            .Where(cp => cp.Category!.Brands.Where(b => b.Id == brandId).Any())
            .Where(cp => cp.Product!.Status != "deleted")
            .Select(cp => cp.Product)
            .OrderByDescending(p => p!.CreateDate)
            .Select(p => new ProductView(p!.Id, p.ImageUrl, p.Name,
                p.BasePrice,
                p.SalePrice)
            ).Skip(page * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<IEnumerable<ProductView>> GetProductsByCategoryId(long categoryId, int page, int pageSize)
    {
        return await _context.CategoryProducts
            .Where(cp => cp.CategoryId == categoryId)
            .Where(cp => cp.Product!.Status != "deleted")
            .Select(cp => cp.Product)
            .OrderByDescending(p => p!.CreateDate)
            .Select(p => new ProductView(p!.Id, p.ImageUrl, p.Name,
                p.BasePrice,
                p.SalePrice)
            ).Skip(page * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<IEnumerable<ProductView>> GetProductsBySeriesId(long seriesId, int page, int pageSize)
    {
        return await _context.CategoryProducts
            .Where(cp => cp.Category!.Brands.Where(b => b.Series.Where(s => s.Id == seriesId).Any()).Any())
            .Where(cp => cp.Product!.Status != "deleted")
            .Select(cp => cp.Product)
            .OrderByDescending(p => p!.CreateDate)
            .Select(p => new ProductView(p!.Id, p.ImageUrl, p.Name,
                p.BasePrice,
                p.SalePrice)
                
            ).Skip(page * pageSize).Take(pageSize).ToListAsync();
    }
}
