using System.Security.Claims;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>Id của user đang đăng nhập (claim NameIdentifier); ném <see cref="UnauthorizedException"/> nếu thiếu.</summary>
        protected string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("User not authenticated");
    }
}
