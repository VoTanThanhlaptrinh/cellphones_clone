using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> HandleResult<T>(ServiceResult<T> serviceResult)
        {
            // Success case
            if (serviceResult.IsSuccess)
            {
                return Ok(new ApiResponse<T>(serviceResult.Message!, serviceResult.Data!));
            }
            // Failure case
            string message = serviceResult.Message!;
            var errorResponse = new ApiResponse<T>(message, default!);
            switch (serviceResult.ErrorType)
            {
                case ServiceErrorType.BadRequest:
                    return StatusCode(StatusCodes.Status400BadRequest, errorResponse);
                case ServiceErrorType.NotFound:
                    return StatusCode(StatusCodes.Status404NotFound, errorResponse);
                case ServiceErrorType.Unauthorized:
                    return StatusCode(StatusCodes.Status401Unauthorized, errorResponse);
                case ServiceErrorType.Forbidden:
                    return StatusCode(StatusCodes.Status403Forbidden, errorResponse);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
