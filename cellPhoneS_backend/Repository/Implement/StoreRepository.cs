using cellphones_backend.Data;
using cellphones_backend.Models;
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
}
