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

    public async Task<ProductViewDetail?> GetProductDetails(long id)
    {
    //     return await _context.Products.Where(p => "active".Equals(p.Status))
    //     .Select(p =>
    //     {
    //         var quantity = p.Stores.FirstOrDefault()?.Quantity ?? 0;
    //         var city = p.Stores.FirstOrDefault()?.StoreHouse?.City;
    //         var district = p.Stores.FirstOrDefault()?.StoreHouse?.District;
    //         var street = p.Stores.FirstOrDefault()?.StoreHouse?.Street;
    //         return new ProductViewDetail
    //          (
    //          p.Id,
    //          p.Name,
    //          p.BasePrice,
    //          p.SalePrice,
    //          p.Version!,
    //          p.ImageUrl,
    //          p.ProductImages.Select(pi => new ImageDTO(pi.Image!.BlobUrl, pi.Image.MimeType, pi.Image.Name, pi.Image.Alt)).ToList(),
    //          p.ProductSpecification.Select(ps => new SpecificationDTO(ps.Specification!.Name, ps.Specification.SpecificationDetails.Select(psd => new SpecificationDetailDTO(psd.Name, psd.Value)).ToList())).ToList(),
    //          p.Commitments.Select(c => c.Context!).ToList(),
    //          quantity,
    //          street, 
    //          district,
    //          city
    //          );
    //     }).FirstOrDefaultAsync(p => p.Id == id);
    // 1. Nên lọc ID ngay từ đầu để SQL chạy nhanh hơn
    return await _context.Products
        .Where(p => p.Id == id && "active".Equals(p.Status))
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

            // --- SỬA LỖI Ở ĐÂY ---
            // Thay vì khai báo var store = ..., ta viết logic lấy dữ liệu trực tiếp:
            
            // Quantity: Lấy số lượng của store đầu tiên, nếu không có thì mặc định 0 (dựa vào tính chất int default)
            p.Stores.Select(s => s.Quantity).FirstOrDefault(), 
            
            // Street: Lấy Store -> StoreHouse -> Street
            p.Stores.Select(s => s.StoreHouse != null ? s.StoreHouse.Street : null).FirstOrDefault(),
            
            // District
            p.Stores.Select(s => s.StoreHouse != null ? s.StoreHouse.District : null).FirstOrDefault(),
            
            // City
            p.Stores.Select(s => s.StoreHouse != null ? s.StoreHouse.City : null).FirstOrDefault()
        ))
        .FirstOrDefaultAsync();
    }
}
    
