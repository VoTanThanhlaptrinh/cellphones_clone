using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/cartdetail")]
    [ApiController]
    public class CartDetailController : ControllerBase
    {
        [HttpGet("{cartId}")]
        public Task<ApiResponse<List<CartDetail>>> GetCartDetails(long cartId)
        {
            // Placeholder logic to retrieve cart details by cart ID
            return null!;
        }
        [HttpGet("/{userId}/{productId}/{quantity}")]
        public Task<ApiResponse<string>> AddToCart(string userId, long productId, int quantity)
        {
            // Placeholder logic to add product to cart
            return null!;
        }
        [HttpDelete("/{userId}/{cartDetailId}")]
        public Task<ApiResponse<string>> RemoveFromCart(string userId, long cartDetailId)
        {
            // Placeholder logic to remove product from cart
            return null!;
        }
    }
}
