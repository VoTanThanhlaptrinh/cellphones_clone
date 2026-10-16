using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/carts")]
    [ApiController]
    public class CartController : BaseController
    {
        private readonly CartService _cartService;
        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<List<CartDetailView>>>> ListCart()
        {
            var userId = await GetUserId();
            var result = await _cartService.GetCartItems(userId);
            return Ok(new ApiResponse<List<CartDetailView>>("Cart items retrieved successfully", result));
        }
        [HttpPost()]
        public async Task<ActionResult<ApiResponse<bool>>> AddProductToCart(CartRequest request)
        {
            var userId = await GetUserId();
            var result = await _cartService.AddToCart(request, userId);
            return Ok(new ApiResponse<bool>("Product added to cart successfully", result));
        }
        [HttpDelete("{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProductOutCart(long cartDetailId)
        {
            var userId = await GetUserId();
             var result = await _cartService.RemoveFromCart(cartDetailId, userId);
             return Ok(new ApiResponse<bool>("Product removed from cart successfully", result));
        }
        [HttpPatch("plusQuantity/{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<int>>> PlusQuantity(long cartDetailId)
        {
            var userId = await GetUserId();
            var result = await _cartService.PlusQuantity(cartDetailId, userId);
            return Ok(new ApiResponse<int>("Plus success", result));
        }
        [HttpPatch("minusQuantity/{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<int>>> MinusQuantity(long cartDetailId)
        {
            var userId = await GetUserId();
            var result = await _cartService.MinusQuantity(cartDetailId, userId);
            return Ok(new ApiResponse<int>("Minus success", result));
        }

        private async Task<string> GetUserId()
        {
            if(User == null || !User.Identity!.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }
    }

}
