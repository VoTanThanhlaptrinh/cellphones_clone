using System;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement;

public class CartDetailServiceImpl : CartDetailService
{
    private readonly ICartDetailRepository _cartDetailRepository;
    public CartDetailServiceImpl(ICartDetailRepository cartDetailRepository)
    {
        _cartDetailRepository = cartDetailRepository;
    }
    public async Task<List<CartDetail>> GetCartDetails(string userId, List<long> cartItemIds)
    {
        return await _cartDetailRepository.GetCartDetails(userId, cartItemIds);
    }
}
