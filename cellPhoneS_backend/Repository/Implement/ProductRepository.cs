using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
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

    public async Task<ProductViewDetail?> GetProductDetails(long id)
    {

        return await _context.Products
            .Where(p => p.Id == id)
            .AsNoTracking()
            .AsSplitQuery()
            .Select(p => new ProductViewDetail(
                p.Id,
                p.Name,
                p.BasePrice,
                p.SalePrice,
                p.Version!,
                p.ImageUrl,
                // Xử lý List Images
                p.ProductImages
                    .Select(pi => new ImageDTO(pi.Image!.BlobUrl, pi.Image.MimeType, pi.Image.Name, pi.Image.Alt))
                    .ToList(),
                // Xử lý List Specifications
                p.ProductSpecification
                    .Select(ps => new SpecificationDTO(
                        ps.Specification!.Name,
                        ps.Specification.SpecificationDetails
                            .Select(psd => new SpecificationDetailDTO(psd.Name, psd.Value))
                            .ToList()
                    )).ToList(),

                // Xử lý Commitments
                p.Commitments.Select(c => c.Context!).ToList(),
                // Xử lý Color
                _context.Stores.Where(s =>
                    s.ProductId == id &&
                    s.Status == "active" &&
                    s.Color != null &&
                    s.Color.Status == "active")
                .Select(s => new ColorDTO(
                    s.Color!.Id, s.Color.Name, new ImageDTO(s.Color.Image!.BlobUrl, s.Color.Image.MimeType, s.Color.Image.Name, s.Color.Image.Alt))
                ).ToList(),
                // Quantity: Lấy số lượng của store đầu tiên, nếu không có thì mặc định 0 (dựa vào tính chất int default) và Địa chỉ cửa hàng
                _context.Stores.Where(s =>
                    s.ProductId == id &&
                    s.Status == "active" &&
                    s.Color != null &&
                    s.Color.Status == "active")
                .Select(s => new StoreHouseDTO(s.Quantity, s.StoreHouse!.Street
                , s.StoreHouse.District, s.StoreHouse.City)).ToList()))
            .FirstOrDefaultAsync();
    }
}