using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Implement;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly CacheService _cacheService;
        private readonly OrderService _orderService;
        public OrderController(CacheService cacheService, OrderService orderService)
        {
            this._cacheService = cacheService;
            this._orderService = orderService;
        }
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
            return null!;
        }

        // POST: api/orders
        [HttpPost("pickup")]
        public async Task<ActionResult<ApiResponse<OrderView>>> CreatePickupOrder([FromBody] PickupOrderRequest payload)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.CreatePickupOrder(userId!, payload);
            return Ok(new ApiResponse<OrderView>("Pickup order created successfully", result));
        }
        [HttpPost("delivery")]
        public async Task<ActionResult<ApiResponse<OrderView>>> CreateDeliveryOrder([FromBody] DeliveryOrderRequest payload)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.CreateDeliveryOrder(userId!, payload);
            return Ok(new ApiResponse<OrderView>("Delivery order created successfully", result));
        }

        // PUT: api/orders/{orderId}
        [HttpPut("{orderId}")]
        public Task<ActionResult<ApiResponse<string>>> UpdateOrder(long orderId, [FromBody] Order order)
        {
            return null!; // TODO: implement order update
        }

        // DELETE: api/orders/{orderId}
        [HttpDelete("{orderId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteOrder(long orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _orderService.DeleteOrder(userId!, orderId);
            return Ok(new ApiResponse<string>("Hủy đơn hàng thành công.", ""));
        }

        [HttpGet("getStores")]
        public async Task<ActionResult<ApiResponse<List<StoreView>>>> GetStores(int year)
        {
            var result = await _cacheService.GetStoreViews();
            return Ok(new ApiResponse<List<StoreView>>("Success", result));
        }
        // GET: api/orders/history/me?pageIndex=1&pageSize=10 — lịch sử mua hàng của người dùng đang đăng nhập
        [HttpGet("history/me")]
        public async Task<ActionResult<ApiResponse<PagedResult<OrderHistoryView>>>> GetMyOrders(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.GetMyOrders(userId!, pageIndex, pageSize);
            return Ok(new ApiResponse<PagedResult<OrderHistoryView>>("Success", result));
        }

        // GET: api/orders/{orderId}/status — trang thanh toán hỏi định kỳ để biết khi nào PayOS báo đã thanh toán
        [HttpGet("{orderId}/status")]
        public async Task<ActionResult<ApiResponse<string>>> GetOrderStatus(long orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var status = await _orderService.GetOrderStatus(userId!, orderId);
            return Ok(new ApiResponse<string>("Success", status));
        }

        [HttpGet("checkout/{orderId}")]
        public async Task<ActionResult<ApiResponse<string>>> Checkout(long orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _orderService.Checkout(userId!, orderId);
            return Ok(new ApiResponse<string>("Order checked out successfully", ""));
        }
    }
}
