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
            var userId = "98a4fdb1-44a7-49d1-b9a0-03f9ff71e3c9";
            return HandleResult(await _cartService.GetCartItems(page,userId));
        }
        [HttpPost()]
        public async Task<ActionResult<ApiResponse<bool>>> AddProductToCart(CartRequest request)
        {
            return HandleResult(await _cartService.AddToCart(request));
        }
        [HttpDelete("{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProductOutCart(long cartDetailId)
        {
             return HandleResult(await _cartService.RemoveFromCart(cartDetailId));
        }
        [HttpPatch("plusQuantity/{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<int>>> PlusQuantity(long cartDetailId)
        {
            return HandleResult(await _cartService.PlusQuantity(cartDetailId));
        }
        [HttpPatch("minusQuantity/{cartDetailId}")]
        public async Task<ActionResult<ApiResponse<int>>> MinusQuantity(long cartDetailId)
        {
            return HandleResult(await _cartService.MinusQuantity(cartDetailId));
        }
        [HttpGet("getStoreViews")]
        public async Task<ActionResult<ApiResponse<List<StoreView>>>> GetStoreViews()
        {
            return HandleResult(await _cartService.GetAllCity());
        }
    }

}
