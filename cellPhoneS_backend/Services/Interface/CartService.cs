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
    Task<ServiceResult<List<CartDetailView>>> GetCartItems(int page, string userId);
    Task<ServiceResult<bool>> AddToCart(CartRequest request);
    Task<ServiceResult<bool>> RemoveFromCart(long cartDetailId);
    Task<ServiceResult<int>> PlusQuantity(long cartDetailId);
    Task<ServiceResult<int>> MinusQuantity(long cartDetailId);

    Task<ServiceResult<Collection<StoreView>>> GetAllCity();
}
