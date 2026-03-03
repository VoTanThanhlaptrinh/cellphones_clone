using cellphones_backend.DTOs.Responses;
using cellphones_backend.Services.Interface;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cellphones_backend.Controllers.User
{
    [Route("api/storeHouses")]
    [ApiController]
    public class StoreHouseController : BaseController
    {
        private readonly StoreHouseService _storeHouseService;

        public StoreHouseController(StoreHouseService storeHouseService)
        {
            _storeHouseService = storeHouseService;
        }

        // GET: api/storehouses/{storeHouseId}
        [HttpGet("{storeHouseId}")]
        public async Task<ActionResult<ApiResponse<StoreHouseResponse>>> GetStoreHouseById(long storeHouseId)
        {
            var result = await _storeHouseService.GetStoreHouseById(storeHouseId);
            return HandleResult(result);
        }

        // GET: api/storehouses
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<StoreHouseResponse>>>> GetAllStoreHouses(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = "active")
        {
            var result = await _storeHouseService.GetAllStoreHouses(pageIndex, pageSize, status);
            return HandleResult(result);
        }
    }
}
