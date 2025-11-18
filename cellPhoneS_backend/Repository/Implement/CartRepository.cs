using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class CartRepository : BaseRepository<Cart>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context) {}

    public Task<bool> CreateCartForUser(string userId)
    {
        Cart cart = new Cart{
            CreateBy = userId,
            Status = "active",
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            UpdateBy = userId
        };
        _context.Carts.Add(cart);
        return Task.FromResult(true);
    }

    public async Task<Cart?> GetCartByUserId(string userId)
    {
        var res  = await _context.Carts.FirstOrDefaultAsync(c => c.CreateBy == userId);
        if (res == null)
          await CreateCartForUser(userId);
        return res;
    }
}
