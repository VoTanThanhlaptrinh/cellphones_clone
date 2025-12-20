using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class CartDetailRepository : BaseRepository<CartDetail>, ICartDetailRepository
{
    public CartDetailRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<CartDetail> GetCartDetailIfExists(long cartId, long productId, long colorId, string userId)
    {
        return await _context.CartDetails.FirstOrDefaultAsync(c => c.CartId == cartId 
                            && c.ProductCartDetailId == productId && c.ColorId == colorId) ?? null!;
    }

    public async Task<List<CartDetailView>> GetCartItems(string userId, int page, int pageSize)
    {
        return await _context.CartDetails.Where(c => c.CreateBy == userId)
            .OrderByDescending(c => c.CreateDate).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CartDetailView(
                        c.Id,
                        c.Quantity,
                        c.Product!.Id,
                        c.Color!.Image!.BlobUrl!,
                        c.Product.Name,
                        c.Product.BasePrice,
                        c.Product.SalePrice)
                    ).ToListAsync();
    }

    public async Task<bool> RemoveCartItems(long cartDetailId)
    {
        var cartDetail = await _context.CartDetails.FindAsync(cartDetailId);
        if (cartDetail == null)
        {
            return false;
        }
        _context.CartDetails.Remove(cartDetail);
        await _context.SaveChangesAsync();
        return true;
    }
}
