using System.Linq.Expressions;
using cellphones_backend.Data;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminCatalogQueryServiceImpl : IAdminCatalogQueryService
{
    private static readonly string[] SortKeys = { "createDate", "updateDate", "name", "id" };
    private readonly ApplicationDbContext _context;

    public AdminCatalogQueryServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }

    // ---------- Categories ----------

    private static readonly Expression<Func<cellphones_backend.Models.Category, AdminCategoryView>> CategoryView = c =>
        new AdminCategoryView(
            c.Id, c.Name, c.SlugName, c.ParentCategoryId, c.ParentCategory!.Name, c.Status,
            c.Children.Count(x => x.Status != RecordStatuses.Deleted),
            c.Brands.Count(b => b.Status != RecordStatuses.Deleted),
            c.Demands.Count(d => d.Status != RecordStatuses.Deleted),
            c.CategoryProducts.Count(cp => cp.Product!.Status == RecordStatuses.Active),
            c.CreateDate, c.UpdateDate);

    public async Task<PagedResult<AdminCategoryView>> GetCategories(AdminCatalogFilter f)
    {
        var status = RecordStatuses.ParseFilter(f.Status);
        var query = _context.Categories.AsNoTracking().AsQueryable();
        query = status == null ? query.Where(c => c.Status != RecordStatuses.Deleted) : query.Where(c => c.Status == status);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var pattern = $"%{f.Search.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Name!, pattern) || EF.Functions.ILike(c.SlugName, pattern));
        }
        if (f.ParentCategoryId == 0) query = query.Where(c => c.ParentCategoryId == null);
        else if (f.ParentCategoryId.HasValue) query = query.Where(c => c.ParentCategoryId == f.ParentCategoryId);

        return await Sort(query, f, c => c.Id, c => c.Name, c => c.CreateDate, c => c.UpdateDate)
            .ThenBy(c => c.Id)
            .Select(CategoryView)
            .ToPagedResultAsync(f.PageIndex, f.PageSize);
    }

    public async Task<AdminCategoryView> GetCategory(long id)
    {
        var row = await _context.Categories.AsNoTracking().Where(c => c.Id == id).Select(CategoryView).FirstOrDefaultAsync();
        return row ?? throw new NotFoundException("Không tìm thấy danh mục.");
    }

    // ---------- Brands ----------

    private static readonly Expression<Func<cellphones_backend.Models.Brand, AdminBrandView>> BrandView = b =>
        new AdminBrandView(
            b.Id, b.Name, b.SlugName, b.CategoryId, b.Category!.Name, b.ImageId, b.Image!.BlobUrl, b.Status,
            b.Series.Count(s => s.Status != RecordStatuses.Deleted),
            b.Category.CategoryProducts.Count(cp => cp.Product!.Status == RecordStatuses.Active),
            b.CreateDate, b.UpdateDate);

    public async Task<PagedResult<AdminBrandView>> GetBrands(AdminCatalogFilter f)
    {
        var status = RecordStatuses.ParseFilter(f.Status);
        var query = _context.Brands.AsNoTracking().AsQueryable();
        query = status == null ? query.Where(b => b.Status != RecordStatuses.Deleted) : query.Where(b => b.Status == status);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var pattern = $"%{f.Search.Trim()}%";
            query = query.Where(b => EF.Functions.ILike(b.Name!, pattern) || EF.Functions.ILike(b.SlugName, pattern));
        }
        if (f.CategoryId.HasValue) query = query.Where(b => b.CategoryId == f.CategoryId);

        return await Sort(query, f, b => b.Id, b => b.Name, b => b.CreateDate, b => b.UpdateDate)
            .ThenBy(b => b.Id)
            .Select(BrandView)
            .ToPagedResultAsync(f.PageIndex, f.PageSize);
    }

    public async Task<AdminBrandView> GetBrand(long id)
    {
        var row = await _context.Brands.AsNoTracking().Where(b => b.Id == id).Select(BrandView).FirstOrDefaultAsync();
        return row ?? throw new NotFoundException("Không tìm thấy thương hiệu.");
    }

    // ---------- Series ----------

    private static readonly Expression<Func<cellphones_backend.Models.Series, AdminSeriesView>> SeriesView = s =>
        new AdminSeriesView(
            s.Id, s.Name, s.SlugName, s.BrandId, s.Brand!.Name, s.Brand.CategoryId, s.Brand.Category!.Name, s.Status,
            s.Brand.Category.CategoryProducts.Count(cp => cp.Product!.Status == RecordStatuses.Active),
            s.CreateDate, s.UpdateDate);

    public async Task<PagedResult<AdminSeriesView>> GetSeries(AdminCatalogFilter f)
    {
        var status = RecordStatuses.ParseFilter(f.Status);
        var query = _context.Series.AsNoTracking().AsQueryable();
        query = status == null ? query.Where(s => s.Status != RecordStatuses.Deleted) : query.Where(s => s.Status == status);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var pattern = $"%{f.Search.Trim()}%";
            query = query.Where(s => EF.Functions.ILike(s.Name!, pattern) || EF.Functions.ILike(s.SlugName, pattern));
        }
        if (f.BrandId.HasValue) query = query.Where(s => s.BrandId == f.BrandId);
        if (f.CategoryId.HasValue) query = query.Where(s => s.Brand!.CategoryId == f.CategoryId);

        return await Sort(query, f, s => s.Id, s => s.Name, s => s.CreateDate, s => s.UpdateDate)
            .ThenBy(s => s.Id)
            .Select(SeriesView)
            .ToPagedResultAsync(f.PageIndex, f.PageSize);
    }

    public async Task<AdminSeriesView> GetSeriesById(long id)
    {
        var row = await _context.Series.AsNoTracking().Where(s => s.Id == id).Select(SeriesView).FirstOrDefaultAsync();
        return row ?? throw new NotFoundException("Không tìm thấy dòng sản phẩm.");
    }

    // ---------- Demands ----------

    private static readonly Expression<Func<cellPhoneS_backend.Models.Demand, AdminDemandView>> DemandView = d =>
        new AdminDemandView(d.Id, d.Name, d.SlugName, d.CategoryId, d.Category!.Name, d.Status, d.CreateDate, d.UpdateDate);

    public async Task<PagedResult<AdminDemandView>> GetDemands(AdminCatalogFilter f)
    {
        var status = RecordStatuses.ParseFilter(f.Status);
        var query = _context.Demands.AsNoTracking().AsQueryable();
        query = status == null ? query.Where(d => d.Status != RecordStatuses.Deleted) : query.Where(d => d.Status == status);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var pattern = $"%{f.Search.Trim()}%";
            query = query.Where(d => EF.Functions.ILike(d.Name!, pattern) || EF.Functions.ILike(d.SlugName, pattern));
        }
        if (f.CategoryId.HasValue) query = query.Where(d => d.CategoryId == f.CategoryId);

        return await Sort(query, f, d => d.Id, d => d.Name, d => d.CreateDate, d => d.UpdateDate)
            .ThenBy(d => d.Id)
            .Select(DemandView)
            .ToPagedResultAsync(f.PageIndex, f.PageSize);
    }

    public async Task<AdminDemandView> GetDemand(long id)
    {
        var row = await _context.Demands.AsNoTracking().Where(d => d.Id == id).Select(DemandView).FirstOrDefaultAsync();
        return row ?? throw new NotFoundException("Không tìm thấy nhu cầu.");
    }

    /// <summary>Whitelist sortBy: createDate (mặc định), updateDate, name, id.</summary>
    private static IOrderedQueryable<T> Sort<T>(IQueryable<T> query, AdminCatalogFilter f,
        Expression<Func<T, long>> id, Expression<Func<T, string?>> name,
        Expression<Func<T, DateTime>> created, Expression<Func<T, DateTime>> updated)
    {
        var desc = QueryOrderingExtensions.IsDescending(f.SortDir);
        return f.SortBy?.Trim().ToLowerInvariant() switch
        {
            null or "" or "createdate" => query.OrderByDirection(created, desc),
            "updatedate" => query.OrderByDirection(updated, desc),
            "name" => query.OrderByDirection(name, desc),
            "id" => query.OrderByDirection(id, desc),
            _ => throw QueryOrderingExtensions.InvalidSort(f.SortBy, SortKeys)
        };
    }
}
