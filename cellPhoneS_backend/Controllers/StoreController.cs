using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Controllers;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers
{
    [Route("api/stores")]
    [ApiController]
    public class StoreController : BaseController
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet("views")]
        public async Task<ActionResult<ApiResponse<List<StoreView>>>> GetStoreViews()
        {
            return HandleResult(await _storeService.GetStoreViewsAsync());
        }
    }
}
