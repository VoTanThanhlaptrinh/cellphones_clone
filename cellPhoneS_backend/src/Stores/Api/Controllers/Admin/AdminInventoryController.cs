using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    /// <summary>Tồn kho theo (kho, sản phẩm, màu): xem, đặt tuyệt đối, điều chỉnh nguyên tử.</summary>
    [Route("api/admin/inventory")]
    [ApiController]
    [RequirePermission(Permissions.InventoryRead)]
    public class AdminInventoryController : BaseController
    {
        private readonly IAdminInventoryService _inventoryService;

        public AdminInventoryController(IAdminInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET: api/admin/inventory?productId=&storeHouseId=&colorId=&lowStock=5&search=&sortBy=updateDate&sortDir=desc&pageIndex=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<InventoryRowView>>>> GetInventory([FromQuery] InventoryFilter filter)
        {
            var result = await _inventoryService.GetInventory(filter);
            return Ok(new ApiResponse<PagedResult<InventoryRowView>>("Success", result));
        }

        // PUT: api/admin/inventory   body { storeHouseId, productId, colorId, quantity }  (đặt tuyệt đối, tạo dòng nếu chưa có)
        [HttpPut]
        [RequirePermission(Permissions.InventoryWrite)]
        public async Task<ActionResult<ApiResponse<string>>> SetInventory([FromBody] SetInventoryRequest request)
        {
            await _inventoryService.SetQuantity(request, CurrentUserId);
            return Ok(new ApiResponse<string>("Inventory updated successfully", ""));
        }

        // POST: api/admin/inventory/adjust   body { storeHouseId, productId, colorId, delta, reason }  -> số lượng mới
        [HttpPost("adjust")]
        [RequirePermission(Permissions.InventoryWrite)]
        public async Task<ActionResult<ApiResponse<int>>> AdjustInventory([FromBody] AdjustInventoryRequest request)
        {
            var quantity = await _inventoryService.Adjust(request, CurrentUserId);
            return Ok(new ApiResponse<int>("Inventory adjusted successfully", quantity));
        }
    }
}
