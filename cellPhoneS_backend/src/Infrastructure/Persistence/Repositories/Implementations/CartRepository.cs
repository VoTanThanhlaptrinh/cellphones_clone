using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class CartRepository : BaseRepository<Cart>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context) {}

    public async Task<Cart> CreateCartForUser(string userId)
    {
        Cart cart = new Cart{
            CreateBy = userId,
            Status = "active",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            UpdateBy = userId
        };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<int> GetAmountCart(string userId)
    {
        return await _context.Carts.Where(c => c.CreateBy == userId).Select(c => c.CartDetails).SelectMany(cd => cd).CountAsync();
    }

    public async Task<Cart?> GetCartByUserIdIfNotThenCreate(string userId)
    {
        var res  = await _context.Carts.FirstOrDefaultAsync(c => c.CreateBy == userId);
        if (res == null)
          res = await CreateCartForUser(userId);
        return res;
    }
}
