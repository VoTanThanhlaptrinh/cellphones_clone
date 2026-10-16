using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services;

public interface CartService
{
    Task<List<CartDetailView>> GetCartItems(string userId);
    Task<bool> AddToCart(CartRequest request, string userId);
    Task<bool> RemoveFromCart(long cartDetailId, string userId);
    Task<int> PlusQuantity(long cartDetailId, string userId);
    Task<int> MinusQuantity(long cartDetailId, string userId);
    Task<int> GetAmountCart(string userId);
}
