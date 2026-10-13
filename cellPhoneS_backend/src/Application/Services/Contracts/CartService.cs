using System;
using System.Collections.ObjectModel;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Elastic.Transport;

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
