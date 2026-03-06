using System.Security.Claims;
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
        private string userId;
        public CartDetailController(CartService cartService)
        {
            this._cartService = cartService;
            userId = "test-user-id"; // Placeholder for user identification
        }
        [HttpGet("{productId}")]
        public async Task<ActionResult<ApiResponse<bool>>> AddToCart(long productId)
        {
            // Placeholder logic to add product to cart
            // return HandleResult(await _cartService.AddToCart(productId, userId));
            return null!;
        }
        [HttpDelete("{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveFromCart(long cartDetailId)
        {
            var userId = await GetUserId();
            return HandleResult(await _cartService.RemoveFromCart(cartDetailId, userId));
        }
        private async Task<string> GetUserId()
        {
            if (User == null || !User.Identity!.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }
    }
}
