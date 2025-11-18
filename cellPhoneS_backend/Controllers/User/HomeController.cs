using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class HomeController : BaseController
    {
        private readonly InitService _initService;

        public HomeController(InitService initService)
        {
            this._initService = initService;
        }
        [HttpGet()]
        public async Task<ActionResult<ApiResponse<HomeViewModel>>> InitHome()
        {
            return HandleResult(await _initService.InitHomePage()); ;
        }

    }
}
