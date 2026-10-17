using cellphones_backend.Models;

namespace cellphones_backend.Repositories;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart> CreateCartForUser(string userId);
    Task<Cart?> GetCartByUserIdIfNotThenCreate(string userId);
    Task<int> GetAmountCart(string userId);
}
