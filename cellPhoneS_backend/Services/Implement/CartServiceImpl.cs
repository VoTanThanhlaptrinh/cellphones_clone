using System;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;

namespace cellPhoneS_backend.Services.Implement;

public class CartServiceImpl : CartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartDetailRepository _cartDetailRepository;
    public CartServiceImpl(ICartRepository cartRepository, ICartDetailRepository cartDetailRepository)
    {
        _cartRepository = cartRepository;
        _cartDetailRepository = cartDetailRepository;
    }

    public async Task<ServiceResult<bool>> AddToCart(long productId, string userId)
    {
        var cart = await _cartRepository.GetCartByUserId(userId);
        if (cart == null)
        {
            return ServiceResult<bool>.Fail("Cart not found", ServiceErrorType.NotFound);
        }
        if(cart.CreateUser == null || cart.CreateUser.Id != userId)
        {
            return ServiceResult<bool>.Fail("User does not own the cart", ServiceErrorType.Unauthorized);
        }
        // chưa có cart detail này -> tạo mới
        // đã có thì +1 vào số lượng
        var cartDetail = await _cartDetailRepository.GetCartDetailIfExists(cart.Id, productId, userId);
        if (cartDetail == null)
        {
            cartDetail = new cellphones_backend.Models.CartDetail
            {
                Cart = cart,
                ProductCartDetailId = productId,
                Quantity = 1,
                CreateBy = userId,
                CreateDate = DateTime.UtcNow,
                UpdateBy = userId,
                UpdateDate = DateTime.UtcNow,
            };
            await _cartDetailRepository.AddAsync(cartDetail);
        }
        else
        {
            cartDetail.Quantity += 1;
            cartDetail.UpdateBy = userId;
            cartDetail.UpdateDate = DateTime.UtcNow;
            await _cartDetailRepository.UpdateAsync(cartDetail);
        }
        return ServiceResult<bool>.Success(true, "Product added to cart successfully");
    }

    public async Task<ServiceResult<List<CartView>>> GetCartItems(int page, int pageSize, string userId)
    {
        var cartItems = await _cartDetailRepository.GetCartItems(userId, page, pageSize);
        if (cartItems == null || cartItems.Count == 0)
        {
            return ServiceResult<List<CartView>>.Fail("No items in cart", ServiceErrorType.NotFound);
        }
        return ServiceResult<List<CartView>>.Success(cartItems, "Cart items retrieved successfully");
    }

    public async Task<ServiceResult<bool>> RemoveFromCart(long cartDetailId)
    {
        var result = await _cartDetailRepository.RemoveCartItems(cartDetailId);
        if (!result)
        {
            return ServiceResult<bool>.Fail("Failed to remove product from cart", ServiceErrorType.NotFound);
        }
        return ServiceResult<bool>.Success(true, "Product removed from cart successfully");
    }
}
