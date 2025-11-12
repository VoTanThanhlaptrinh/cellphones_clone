using cellphones_backend.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/carts")]
    [ApiController]
    public class CartController : ControllerBase
    {
        [HttpGet("all")]
        public Task<ApiResponse<string>> ListCart()
        {
            return null!;
        }
        [HttpGet("{productId}")]
        public Task<ActionResult<ApiResponse<string>>> AddProductTocard(long productId)
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
