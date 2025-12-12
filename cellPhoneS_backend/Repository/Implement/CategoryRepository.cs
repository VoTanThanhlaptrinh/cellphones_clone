using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    private readonly IConfiguration _configuration;
    private string[]? categories;
    private int pageSize;

    public CategoryRepository(ApplicationDbContext context, IConfiguration configuration) : base(context)
    {
        _configuration = configuration;
        pageSize = _configuration.GetValue<int>("DefaultSetting:PageSize");
    }

    public async Task<HomeViewModel> InitHomeByCategories()
    {
        var cateInit = await _context.Categories
                            .Select(c => new CategoryView(c.Id, c.Name,
                            c.Demands.Select(d => new DemandView(d.Id, d.Name!)).ToList(),
                            c.Brands.Select(b => new BrandView(b.Id, b.Name!)).ToList(),
                            c.CategoryProducts!.Select(cp => cp.Product)
                                .OrderByDescending(p => p!.CreateDate)
                                .Select(product => new ProductView
                                (
                                    product!.Id,
                                    product!.ImageUrl,
                                    product.Name,
                                    product.BasePrice,
                                    product.SalePrice
                                ))
                                .Take(pageSize))).ToListAsync();
        return new HomeViewModel(cateInit);
    }
}
