using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface ICartRepository : IRepository<Cart>
{
    Task<bool> CreateCartForUser(string userId);
    Task<Cart?> GetCartByUserId(string userId);
}
