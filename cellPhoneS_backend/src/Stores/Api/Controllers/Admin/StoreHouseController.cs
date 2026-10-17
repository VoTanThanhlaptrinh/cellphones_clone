using System.Security.Claims;
using cellPhoneS_backend.Constants;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Filters;
using cellphones_backend.Services.Interface;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/storehouses")]
    [ApiController]
    [RequirePermission(Permissions.InventoryRead)]
    public class AdminStoreHouseController : BaseController
    {
        private readonly StoreHouseService _storeHouseService;

        public AdminStoreHouseController(StoreHouseService storeHouseService)
        {
            _storeHouseService = storeHouseService;
        }

        // POST: api/admin/storehouses
        [HttpPost]
        [RequirePermission(Permissions.InventoryWrite)]
        public async Task<ActionResult<ApiResponse<long>>> CreateStoreHouse([FromBody] CreateStoreHouseRequest createStoreHouseRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _storeHouseService.CreateStoreHouse(createStoreHouseRequest, userId);
            return Ok(new ApiResponse<long>("Storehouse created successfully", result));
        }

        // PUT: api/admin/storehouses/{storeHouseId}
        [HttpPut("{storeHouseId}")]
        [RequirePermission(Permissions.InventoryWrite)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStoreHouse(
            long storeHouseId, 
            [FromBody] UpdateStoreHouseRequest updateStoreHouseRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            // Prevent ID tampering
            if (storeHouseId != updateStoreHouseRequest.Id)
            {
                return BadRequest(new ApiResponse<string>("StoreHouse ID in URL does not match ID in request body", null!));
            }
            
            await _storeHouseService.UpdateStoreHouse(storeHouseId, updateStoreHouseRequest, userId);
            return Ok(new ApiResponse<string>("Storehouse updated successfully", ""));
        }

        // DELETE: api/admin/storehouses/{storeHouseId}
        [HttpDelete("{storeHouseId}")]
        [RequirePermission(Permissions.InventoryWrite)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteStoreHouse(long storeHouseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            await _storeHouseService.DeleteStoreHouse(storeHouseId, userId);
            return Ok(new ApiResponse<string>("Storehouse deleted successfully", ""));
        }
    }
}
