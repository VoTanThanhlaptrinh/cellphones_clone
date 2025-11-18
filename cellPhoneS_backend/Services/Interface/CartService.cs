using System;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;

namespace cellPhoneS_backend.Services;

public interface CartService
{
    Task<ServiceResult<List<CartView>>> GetCartItems(int page, int pageSize, string userId);
    Task<ServiceResult<bool>> AddToCart(long productId, string userId);
    Task<ServiceResult<bool>> RemoveFromCart(long cartDetailId);
}
