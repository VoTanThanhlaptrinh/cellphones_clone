using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services.Interface;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cellphones_backend.Controllers.Admin
{
    [Route("api/admin/storehouses")]
    [ApiController]
    public class AdminStoreHouseController : BaseController
    {
        private readonly StoreHouseService _storeHouseService;

        public AdminStoreHouseController(StoreHouseService storeHouseService)
        {
            _storeHouseService = storeHouseService;
        }

        // POST: api/admin/storehouses
        [HttpPost]
        public async Task<ActionResult<ApiResponse<long>>> CreateStoreHouse([FromBody] CreateStoreHouseRequest createStoreHouseRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<long>("User not authenticated", 0));
            }
            
            var result = await _storeHouseService.CreateStoreHouse(createStoreHouseRequest, userId);
            return HandleResult(result);
        }

        // PUT: api/admin/storehouses/{storeHouseId}
        [HttpPut("{storeHouseId}")]
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
            
            var result = await _storeHouseService.UpdateStoreHouse(storeHouseId, updateStoreHouseRequest, userId);
            return HandleResult(result);
        }

        // DELETE: api/admin/storehouses/{storeHouseId}
        [HttpDelete("{storeHouseId}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteStoreHouse(long storeHouseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<string>("User not authenticated", null!));
            }
            
            var result = await _storeHouseService.DeleteStoreHouse(storeHouseId, userId);
            return HandleResult(result);
        }
    }
}
