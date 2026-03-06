using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers
{
    [Route("api/stores")]
    [ApiController]
    public class StoreController : BaseController
    {
        private readonly IStoreService _storeService;
        private readonly IVietnamLocationService _vietnamLocationService;

        public StoreController(IStoreService storeService, IVietnamLocationService vietnamLocationService)
        {
            _storeService = storeService;
            _vietnamLocationService = vietnamLocationService;
        }

        [HttpGet("views")]
        public async Task<ActionResult<ApiResponse<List<StoreView>>>> GetStoreViews()
        {
            return HandleResult(await _storeService.GetStoreViewsAsync());
        }

        [HttpGet("provinces")]
        public async Task<ActionResult<ApiResponse<List<VietnamProvinceDto>>>> GetVietnamLocations(
            [FromQuery] GetVietnamLocationsQuery query,
            CancellationToken cancellationToken)
        {
            return HandleResult(await _vietnamLocationService.GetLocationsAsync(query.depth, cancellationToken));
        }
    }
}
