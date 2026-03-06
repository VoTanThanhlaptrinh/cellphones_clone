using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
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

    public async Task<List<Store>> AllocateAllStockAsync(List<CartDetail> cartItems, string customerProvince)
    {
        // 1. Lấy danh sách ProductId để lọc sơ bộ ở DB (Big Filter)
        var productIds = cartItems.Select(ci => ci.ProductCartDetailId).Distinct().ToList();

        // 2. TRUY VẤN DUY NHẤT: Lấy toàn bộ kho của những sản phẩm này
        // Ta lấy hết các màu của những sản phẩm đó về RAM để xử lý cho nhanh
        var allPotentialStores = await _context.Stores
            .Include(s => s.StoreHouse)
            .Where(s => productIds.Contains(s.ProductId) && s.Quantity > 0)
            .ToListAsync();


        return allPotentialStores;
    }

    public async Task<List<Store>> GetInventoryForStoreAsync(List<CartDetail> cartItems, long storeHouseId)
    {
        // 1. Lấy danh sách ProductId để lọc sơ bộ ở DB (Big Filter)
        var productIds = cartItems.Select(ci => ci.ProductCartDetailId).Distinct().ToList();

        // 2. TRUY VẤN DUY NHẤT: Lấy kho của cửa hàng cụ thể
        var storeInventory = await _context.Stores
                .Include(s => s.StoreHouse)
                .Where(s => productIds.Contains(s.ProductId) && s.Quantity > 0)
                .ToListAsync();

        return storeInventory;
    }
}