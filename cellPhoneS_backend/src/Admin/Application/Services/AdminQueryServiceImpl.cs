using cellphones_backend.Data;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminQueryServiceImpl : IAdminQueryService
{
    private readonly ApplicationDbContext _context;

    public AdminQueryServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminProductView>> GetProducts(int pageIndex, int pageSize, string? search)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name!, pattern));
        }

        return await query
            .OrderByDescending(p => p.CreateDate)
            .Select(p => new AdminProductView(
                p.Id, p.Name, p.ImageUrl, p.BasePrice, p.SalePrice, p.Status,
                p.Stores.Sum(s => s.Quantity), p.CreateDate))
            .ToPagedResultAsync(pageIndex, pageSize);
    }
}
