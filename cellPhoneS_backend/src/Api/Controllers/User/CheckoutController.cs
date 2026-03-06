using Microsoft.AspNetCore.Mvc;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;

namespace cellPhoneS_backend.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        // POST: api/checkout/{orderId}
        // Accepts existing order id to perform checkout confirmation/processing
        [HttpGet("{orderId}")]
        public Task<ActionResult<ApiResponse<string>>> Checkout(long orderId)
        {
            return null!; // TODO: implement checkout processing by orderId
        }
    }
}
