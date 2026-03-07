using System.Text;
using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class StoreRepository : BaseRepository<Store>, IStoreRepository
{
    public StoreRepository(ApplicationDbContext context) : base(context) { }

    public async Task<int> GetTotalQuantityInStoreByProductAndColor(long productCartDetailId, long colorId)
    {
        var stock = await _context.ProductColorStockView
            .FirstOrDefaultAsync(x => x.ProductId == productCartDetailId && x.ColorId == colorId);
        return stock?.TotalQuantity ?? 0;
    }
    public async Task<Store?> FindByProductAndColorAsync(long productCartDetailId, long colorId)
    {
        return await _context.Stores.Include(s => s.StoreHouse)
            .FirstOrDefaultAsync(s => s.ColorId == colorId && s.ProductId == productCartDetailId);
    }

    public async Task<List<StoreInventoryDTO>> AllocateAllStockAsync(List<CartDetail> cartItems)
    {
        // 1. Lấy danh sách ProductId để lọc sơ bộ ở DB (Big Filter)
        var productIds = cartItems.Select(ci => ci.ProductCartDetailId).Distinct().ToList();
        // 2. TRUY VẤN DUY NHẤT: Lấy toàn bộ kho của những sản phẩm này
        var allPotentialStores = await _context.Stores
            .Where(s => productIds.Contains(s.ProductId) && s.Quantity > 0)
            .Select(s => new StoreInventoryDTO
            {
                StoreHouseId = s.StoreHouseId,
                ProductId = s.ProductId,
                ColorId = s.ColorId,
                Quantity = s.Quantity,

                // EF Core tự Join để lấy các trường này, nhưng không load cả Object StoreHouse nặng nề
                City = s.StoreHouse != null ? s.StoreHouse.City : null,
                District = s.StoreHouse != null ? s.StoreHouse.District : null,

                ProductName = s.Product != null ? s.Product.Name : null,
                SalePrice = s.Product != null ? s.Product.SalePrice : 0,
                ColorName = s.Color != null ? s.Color.Name : null,
                ProductImageUrl = s.Product != null ? s.Product.ImageUrl : null,
                ColorImageUrl = s.Color != null && s.Color.Image != null ? s.Color.Image.BlobUrl : null
            })
        .AsNoTracking() // Quan trọng: Không cần theo dõi vì đây là DTO
        .ToListAsync();

        return allPotentialStores;
    }

    public Task<List<Store>> GetStoresAsync(List<OrderDetail> orderDetails)
    {
        var storeHouseIds = orderDetails.Select(od => od.StoreHouseId).Distinct().ToList();
        var productIds = orderDetails.Select(od => od.ProductOrderDetailId).Distinct().ToList();
        var colorIds = orderDetails.Select(od => od.ColorId).Distinct().ToList();
        return _context.Stores.Where(s => storeHouseIds.Contains(s.StoreHouseId) &&
                        productIds.Contains(s.ProductId) &&
                        colorIds.Contains(s.ColorId))
            .ToListAsync();
    }

    public async Task UpdateRangeAsync(List<StoreInventoryDTO> modifiedStores)
    {
        if (modifiedStores == null || !modifiedStores.Any()) return;

        // Chia đợt để tránh làm nghẽn Database (Chunking)
        const int chunkSize = 50;
        var currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        for (int i = 0; i < modifiedStores.Count; i += chunkSize)
        {
            var chunk = modifiedStores.Skip(i).Take(chunkSize);
            var sqlBatch = new StringBuilder();

            foreach (var s in chunk)
            {
                // Cập nhật giá trị TUYỆT ĐỐI từ DTO vào Database
                sqlBatch.Append($@"
                UPDATE ""Stores"" 
                SET ""Quantity"" = {s.Quantity}, 
                    ""UpdateDate"" = '{currentTime}'
                WHERE ""StoreHouseId"" = {s.StoreHouseId} 
                  AND ""ProductId"" = {s.ProductId} 
                  AND ""ColorId"" = {s.ColorId};
            ");
            }

            // Thực thi lô lệnh UPDATE cho đợt này
            if (sqlBatch.Length > 0)
            {
                await _context.Database.ExecuteSqlRawAsync(sqlBatch.ToString());
            }
        }
    }
}