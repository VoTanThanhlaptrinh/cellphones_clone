using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/carts")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }
        // [HttpGet("")]
        // public Task<ApiResponse<string>> ListCart()
        // {
        //     return null!;
        // }
        [HttpGet("{productId}")]
        public Task<ActionResult<ApiResponse<string>>> AddProductToCart(long productId)
        {
            return null!;
        }
        [HttpDelete("{productId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteProductOutCart(long productId)
        {
            return null!;
        }
        [HttpPut("{productId}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateProductInCart(long productId)
        {
            return null!;
        }
    }
}
