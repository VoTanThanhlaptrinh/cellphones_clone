using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/carts/details")]
    [ApiController]
    public class CartDetailController : BaseController
    {
        private readonly CartService _cartService;
        private  string userId;
        public CartDetailController(CartService cartService)
        {
            this._cartService = cartService;
            userId = "test-user-id"; // Placeholder for user identification
        }
        [HttpGet("{page}/{pageSize}")]
        public async Task<ActionResult<ApiResponse<List<CartView>>>> GetCartDetails(int page, int pageSize)
        {
            return HandleResult(await _cartService.GetCartItems(page, pageSize, userId));
        }
        [HttpGet("{productId}")]
        public async Task<ActionResult<ApiResponse<bool>>> AddToCart(long productId)
        {
            // Placeholder logic to add product to cart
            return HandleResult(await _cartService.AddToCart(productId, userId));
        }
        [HttpDelete("{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveFromCart(long cartDetailId)
        {
            // Placeholder logic to remove product from cart
            return HandleResult(await _cartService.RemoveFromCart(cartDetailId));
        }
    }
}
