using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly InitService _initService;

        public HomeController(InitService initService)
        {
            this._initService = initService;
        }
        [HttpGet()]
        public Task<ApiResponse<HomeViewModel>> InitHome()
        {
            return this._initService.InitHomePage();
        }

    }
}
