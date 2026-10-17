using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;

namespace cellphones_backend.Repositories;

public interface ICartDetailRepository : IRepository<CartDetail>
{
    Task<CartDetail> GetCartDetailIfExists(long cartId, long productId, long colorId, string userId);
    Task<List<CartDetailView>> GetCartItems(string userId);
    Task<bool> RemoveCartItems(long cartDetailId);
    Task<List<CartDetail>> GetCartDetails(string userId, List<long> cartItemIds);
}
