using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace cellPhoneS_backend.Filters
{
    /// <summary>
    /// Bắt các exception nghiệp vụ từ service và trả về <see cref="ApiResponse{T}"/> với status tương ứng.
    /// Dùng filter của MVC (thay vì middleware) để giữ nguyên các header CORS trên response lỗi.
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var statusCode = context.Exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                UnauthorizedException or UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                InternalServerException => StatusCodes.Status500InternalServerError,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                _ => (int?)null
            };

            // Exception không phải lỗi nghiệp vụ: để ASP.NET Core xử lý như bình thường (500)
            if (statusCode is null)
            {
                return;
            }

            _logger.LogInformation("{ExceptionType} -> {StatusCode}: {Message}",
                context.Exception.GetType().Name, statusCode, context.Exception.Message);

            context.Result = new ObjectResult(new ApiResponse<object?>(context.Exception.Message, null))
            {
                StatusCode = statusCode
            };
            context.ExceptionHandled = true;
        }
    }
}
