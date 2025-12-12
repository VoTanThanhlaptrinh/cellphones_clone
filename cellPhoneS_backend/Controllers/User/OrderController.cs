using Microsoft.AspNetCore.Mvc;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;

namespace cellphones_backend.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        // GET: api/orders/list/{page}/{pageSize}
        [HttpGet("list/{page}/{pageSize}")]
        public Task<ActionResult<ApiResponse<List<Order>>>> GetOrders(int page, int pageSize)
        {
            return null!; // TODO: implement listing with pagination
        }

        // GET: api/orders/{orderId}
        [HttpGet("{orderId}")]
        public Task<ActionResult<ApiResponse<Order>>> GetOrder(long orderId)
        {
            return null!; // TODO: implement single order retrieval
        }

        // POST: api/orders
        [HttpPost]
        public Task<ActionResult<ApiResponse<string>>> CreateOrder([FromBody] Order order)
        {
            return null!; // TODO: implement order creation
        }

        // PUT: api/orders/{orderId}
        [HttpPut("{orderId}")]
        public Task<ActionResult<ApiResponse<string>>> UpdateOrder(long orderId, [FromBody] Order order)
        {
            return null!; // TODO: implement order update
        }

        // DELETE: api/orders/{orderId}
        [HttpDelete("{orderId}")]
        public Task<ActionResult<ApiResponse<string>>> DeleteOrder(long orderId)
        {
            return null!; // TODO: implement order deletion
        }

        // GET: api/orders/today
        [HttpGet("today")]
        public Task<ActionResult<ApiResponse<List<Order>>>> GetTodayOrders()
        {
            return null!; // TODO: implement fetch orders created today
        }

        // GET: api/orders/month/{year}/{month}
        [HttpGet("month/{year:int}/{month:int}")]
        public Task<ActionResult<ApiResponse<List<Order>>>> GetMonthOrders(int year, int month)
        {
            return null!; // TODO: implement fetch orders by month
        }

        // GET: api/orders/quarter/{year}/{quarter}
        [HttpGet("quarter/{year:int}/{quarter:int}")]
        public Task<ActionResult<ApiResponse<List<Order>>>> GetQuarterOrders(int year, int quarter)
        {
            return null!; // TODO: implement fetch orders by quarter (1-4)
        }

        // GET: api/orders/year/{year}
        [HttpGet("year/{year:int}")]
        public Task<ActionResult<ApiResponse<List<Order>>>> GetYearOrders(int year)
        {
            return null!; // TODO: implement fetch orders by entire year
        }
    }
}
