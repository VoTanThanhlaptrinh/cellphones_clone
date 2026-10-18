using cellphones_backend.Data;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminProductQueryServiceImpl : IAdminProductQueryService
{
    private static readonly string[] SortKeys = { "createDate", "updateDate", "name", "salePrice", "basePrice", "stock", "id" };
    private readonly ApplicationDbContext _context;

    public AdminProductQueryServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminProductView>> GetProducts(AdminProductFilter filter)
    {
        var status = RecordStatuses.ParseFilter(filter.Status);
        var desc = QueryOrderingExtensions.IsDescending(filter.SortDir);
        var query = _context.Products.AsNoTracking().AsQueryable();

        query = status == null
            ? query.Where(p => p.Status != RecordStatuses.Deleted)
            : query.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name!, pattern));
        }
        if (filter.CategoryId.HasValue)
        {
            var categoryId = filter.CategoryId.Value;
            query = query.Where(p => p.CategoryProducts.Any(cp => cp.CategoryId == categoryId));
        }
        if (filter.BrandId.HasValue)
        {
            var brandId = filter.BrandId.Value;
            query = query.Where(p => p.CategoryProducts.Any(cp => cp.Category!.Brands.Any(b => b.Id == brandId)));
        }
        if (filter.LowStock.HasValue)
        {
            var threshold = filter.LowStock.Value;
            query = query.Where(p => (p.Stores.Sum(s => (int?)s.Quantity) ?? 0) <= threshold);
        }

        var ordered = (filter.SortBy?.Trim().ToLowerInvariant()) switch
        {
            null or "" or "createdate" => query.OrderByDirection(p => p.CreateDate, desc),
            "updatedate" => query.OrderByDirection(p => p.UpdateDate, desc),
            "name" => query.OrderByDirection(p => p.Name, desc),
            "saleprice" => query.OrderByDirection(p => p.SalePrice, desc),
            "baseprice" => query.OrderByDirection(p => p.BasePrice, desc),
            "stock" => query.OrderByDirection(p => p.Stores.Sum(s => (int?)s.Quantity) ?? 0, desc),
            "id" => query.OrderByDirection(p => p.Id, desc),
            _ => throw QueryOrderingExtensions.InvalidSort(filter.SortBy, SortKeys)
        };

        return await ordered.ThenBy(p => p.Id)
            .Select(p => new AdminProductView(
                p.Id, p.Name, p.ImageUrl, p.BasePrice, p.SalePrice, p.Status,
                p.Stores.Sum(s => (int?)s.Quantity) ?? 0,
                p.CategoryProducts.Where(cp => cp.Status != RecordStatuses.Deleted).Select(cp => cp.Category!.Name!).ToList(),
                p.CreateDate, p.UpdateDate))
            .ToPagedResultAsync(filter.PageIndex, filter.PageSize);
    }

    public async Task<AdminProductDetail> GetProduct(long productId)
    {
        var detail = await _context.Products.AsNoTracking().AsSplitQuery()
            .Where(p => p.Id == productId)
            .Select(p => new AdminProductDetail(
                p.Id, p.Name, p.Version, p.BasePrice, p.SalePrice, p.Status, p.ImageUrl,
                p.Stores.Sum(s => (int?)s.Quantity) ?? 0,
                p.CreateDate, p.UpdateDate,
                p.ProductImages.Where(pi => pi.Status != RecordStatuses.Deleted)
                    .Select(pi => new AdminProductImageView(pi.ImageId, pi.Image!.BlobUrl, pi.Image.Name, pi.Image.Alt, pi.Image.MimeType, pi.Status))
                    .ToList(),
                new List<AdminProductColorView>(),
                p.ProductSpecification.Where(ps => ps.Status != RecordStatuses.Deleted)
                    .Select(ps => new AdminProductSpecificationView(
                        ps.SpecificationId, ps.Specification!.Name,
                        ps.Specification.SpecificationDetails.Where(d => d.Status != RecordStatuses.Deleted)
                            .Select(d => new AdminSpecificationDetailView(d.Id, d.Name, d.Value)).ToList()))
                    .ToList(),
                p.Commitments.Where(c => c.Status != RecordStatuses.Deleted)
                    .Select(c => new AdminProductCommitmentView(c.Id, c.Context, c.Status)).ToList(),
                p.CategoryProducts.Where(cp => cp.Status != RecordStatuses.Deleted)
                    .Select(cp => new AdminProductCategoryView(cp.CategoryId, cp.Category!.Name)).ToList(),
                p.Stores.OrderBy(s => s.StoreHouseId).ThenBy(s => s.ColorId)
                    .Select(s => new AdminProductStockView(
                        s.StoreHouseId, s.StoreHouse!.Street + ", " + s.StoreHouse.District + ", " + s.StoreHouse.City,
                        s.ColorId, s.Color!.Name, s.Quantity, s.Status))
                    .ToList()))
            .FirstOrDefaultAsync();

        if (detail == null) throw new NotFoundException("Không tìm thấy sản phẩm.");

        // Màu của sản phẩm chỉ tồn tại qua các dòng tồn kho (Store), nên lấy từ đó.
        var colors = await _context.Stores.AsNoTracking()
            .Where(s => s.ProductId == productId && s.Color!.Status != RecordStatuses.Deleted)
            .Select(s => new AdminProductColorView(s.ColorId, s.Color!.Name, s.Color.Image!.BlobUrl, s.Color.Status))
            .Distinct()
            .ToListAsync();
        return detail with { Colors = colors };
    }
}
