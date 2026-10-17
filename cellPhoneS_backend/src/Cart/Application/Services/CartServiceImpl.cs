using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
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

    public async Task<bool> AddToCart([FromBody] CartRequest request, string userId)
    {

        var cart = await _cartRepository.GetCartByUserIdIfNotThenCreate(userId);

        var productId = request.productId;
        var colorId = request.colorId;
        // validate color exists
        var colorExists = await _colorRepository.FindById(colorId);
        Console.WriteLine(colorExists);
        if (colorExists == null)
        {
            throw new BadRequestException("Color not found");
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
        return true;
    }

    public async Task<int> GetAmountCart(string userId)
    {
        return await _cartRepository.GetAmountCart(userId);
    }

    public async Task<List<CartDetailView>> GetCartItems(string userId)
    {
        // Giỏ hàng trống không phải lỗi: trả về danh sách rỗng với 200
        var cartItems = await _cartDetailRepository.GetCartItems(userId);
        return cartItems ?? new List<CartDetailView>();
    }

    public async Task<int> MinusQuantity(long cartDetailId, string userId)
    {
            var cartDetail = await _cartDetailRepository.GetByIdAsync(cartDetailId);
        if (cartDetail == null)
            throw new BadRequestException("Cart detail not found");
        if (cartDetail.Quantity < 2)
            throw new BadRequestException("Quantity at least one");
        cartDetail.Quantity = cartDetail.Quantity - 1;
        await _cartDetailRepository.UpdateAsync(cartDetail);
        await _cartDetailRepository.SaveChangesAsync();
        return cartDetail.Quantity;
    }

    public async Task<int> PlusQuantity(long cartDetailId, string userId)
    {
        var cartDetail = await _cartDetailRepository.GetByIdAsync(cartDetailId);
        if (cartDetail == null)
            throw new BadRequestException("Cart detail not found");
        var totalQuantity = await _storeRepository.GetTotalQuantityInStoreByProductAndColor(cartDetail.ProductCartDetailId, cartDetail.ColorId);
        if (cartDetail.Quantity == totalQuantity)
            throw new BadRequestException("We don't have any more item");
        cartDetail.Quantity += 1;
        await _cartDetailRepository.UpdateAsync(cartDetail);
        await _cartDetailRepository.SaveChangesAsync();
        return cartDetail.Quantity;
    }

    public async Task<bool> RemoveFromCart(long cartDetailId, string userId)
    {
        var result = await _cartDetailRepository.RemoveCartItems(cartDetailId);
        if (!result)
        {
            throw new NotFoundException("Failed to remove product from cart");
        }
        return true;
    }
}
