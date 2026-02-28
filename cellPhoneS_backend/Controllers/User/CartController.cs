using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;
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
        [HttpGet("{page}")]
        public async Task<ActionResult<ApiResponse<List<CartDetailView>>>> ListCart(int page)
        {
            var userId = await GetUserId();
            return HandleResult(await _cartService.GetCartItems(page, userId));
        }
        [HttpPost()]
        public async Task<ActionResult<ApiResponse<bool>>> AddProductToCart(CartRequest request)
        {
            var userId = await GetUserId();
            return HandleResult(await _cartService.AddToCart(request, userId));
        }
        [HttpDelete("{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProductOutCart(long cartDetailId)
        {
            var userId = await GetUserId();
             return HandleResult(await _cartService.RemoveFromCart(cartDetailId, userId));
        }
        [HttpPatch("plusQuantity/{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<int>>> PlusQuantity(long cartDetailId)
        {
            var userId = await GetUserId();
            return HandleResult(await _cartService.PlusQuantity(cartDetailId, userId));
        }
        [HttpPatch("minusQuantity/{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<int>>> MinusQuantity(long cartDetailId)
        {
            var userId = await GetUserId();
            return HandleResult(await _cartService.MinusQuantity(cartDetailId, userId));
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
