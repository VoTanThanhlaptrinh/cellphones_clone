using System;
using System.Collections.ObjectModel;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using Elastic.Transport;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Services.Implement;
public class CartServiceImpl : CartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartDetailRepository _cartDetailRepository;
    private readonly IColorRepository _colorRepository;
    private readonly IConfiguration _configuration;
    private readonly IStoreRepository _storeRepository;
    private int _pageSize;
    public CartServiceImpl(ICartRepository cartRepository, ICartDetailRepository cartDetailRepository
    , IColorRepository colorRepository, IConfiguration configuration, IStoreRepository storeRepository)
    {
        _cartRepository = cartRepository;
        _cartDetailRepository = cartDetailRepository;
        _colorRepository = colorRepository;
        _configuration = configuration;
        _pageSize = _configuration.GetValue<int>("DefaultSetting:PageSize");
        _storeRepository = storeRepository;
    }

    public async Task<ServiceResult<bool>> AddToCart([FromBody] CartRequest request, string userId)
    {

        var cart = await _cartRepository.GetCartByUserIdIfNotThenCreate(userId);

        var productId = request.productId;
        var colorId = request.colorId;
        // validate color exists
        var colorExists = await _colorRepository.FindById(colorId);
        Console.WriteLine(colorExists);
        if (colorExists == null)
        {
            return ServiceResult<bool>.Fail(productId.ToString(), ServiceErrorType.BadRequest);
        }

        var cartDetail = await _cartDetailRepository
            .GetCartDetailIfExists(cart!.Id, productId, colorId, userId);

        if (cartDetail == null)
        {
            cartDetail = new CartDetail
            {
                Cart = cart,
                ProductCartDetailId = productId,
                ColorId = colorId,
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

        await _cartDetailRepository.SaveChangesAsync();
        return ServiceResult<bool>.Success(true, "Product added to cart successfully");
    }

    public async Task<ServiceResult<int>> GetAmountCart(string userId)
    {
        var amount = await _cartRepository.GetAmountCart(userId);
        return ServiceResult<int>.Success(amount, "Amount cart retrieved successfully");
    }

    public async Task<ServiceResult<List<CartDetailView>>> GetCartItems(int page, string userId)
    {
        var cartItems = await _cartDetailRepository.GetCartItems(userId, page, _pageSize);
        if (cartItems == null || cartItems.Count == 0)
        {
            return ServiceResult<List<CartDetailView>>.Fail("No items in cart", ServiceErrorType.NotFound);
        }
        return ServiceResult<List<CartDetailView>>.Success(cartItems, "Cart items retrieved successfully");
    }

    public async Task<ServiceResult<int>> MinusQuantity(long cartDetailId, string userId)
    {
            var cartDetail = await _cartDetailRepository.GetByIdAsync(cartDetailId);
        if (cartDetail == null)
            return ServiceResult<int>.Fail("Cart detail not found", ServiceErrorType.BadRequest);
        if (cartDetail.Quantity < 2)
            return ServiceResult<int>.Fail("Quantity at least one", ServiceErrorType.BadRequest);
        cartDetail.Quantity = cartDetail.Quantity - 1;
        await _cartDetailRepository.UpdateAsync(cartDetail);
        await _cartDetailRepository.SaveChangesAsync();
        return ServiceResult<int>.Success(cartDetail.Quantity, "Minus success");
    }

    public async Task<ServiceResult<int>> PlusQuantity(long cartDetailId, string userId)
    {
        var cartDetail = await _cartDetailRepository.GetByIdAsync(cartDetailId);
        if (cartDetail == null)
            return ServiceResult<int>.Fail("Cart detail not found", ServiceErrorType.BadRequest);
        var totalQuantity = await _storeRepository.GetTotalQuantityInStoreByProductAndColor(cartDetail.ProductCartDetailId, cartDetail.ColorId);
        if (cartDetail.Quantity == totalQuantity)
            return ServiceResult<int>.Fail("We don't have any more item", ServiceErrorType.BadRequest);
        cartDetail.Quantity += 1;
        await _cartDetailRepository.UpdateAsync(cartDetail);
        await _cartDetailRepository.SaveChangesAsync();
        return ServiceResult<int>.Success(cartDetail.Quantity, "Plus success");
    }

    public async Task<ServiceResult<bool>> RemoveFromCart(long cartDetailId, string userId)
    {
        var result = await _cartDetailRepository.RemoveCartItems(cartDetailId);
        if (!result)
        {
            return ServiceResult<bool>.Fail("Failed to remove product from cart", ServiceErrorType.NotFound);
        }
        return ServiceResult<bool>.Success(true, "Product removed from cart successfully");
    }
}
