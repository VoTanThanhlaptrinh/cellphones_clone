using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers.User
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("healthcheck")]
        public IActionResult CheckHealth()
        {
            return Ok("Service is up and running!");
        }
    }
}
