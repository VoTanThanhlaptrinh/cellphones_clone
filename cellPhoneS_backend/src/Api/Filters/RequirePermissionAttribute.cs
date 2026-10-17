using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace cellPhoneS_backend.Filters
{
    /// <summary>
    /// Yêu cầu user hiện tại có các permission đã cho (tra qua role trong JWT). Đặt trên action hoặc controller;
    /// nhiều attribute (vd. controller + action) thì phải thoả tất cả.
    /// Lớp thô `/api/admin/*` (admin.access) vẫn do <c>CentralizedAuthMiddleware</c> chặn trước.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public IReadOnlyList<string> RequiredPermissions { get; }

        public RequirePermissionAttribute(params string[] permissions) => RequiredPermissions = permissions;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedObjectResult(new ApiResponse<object?>("Unauthorized: Authentication required.", null));
                return;
            }

            var service = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            var granted = await service.GetPermissionsAsync(user.FindAll(ClaimTypes.Role).Select(c => c.Value));
            if (!RequiredPermissions.All(granted.Contains))
            {
                context.Result = new ObjectResult(new ApiResponse<object?>("Forbidden: Insufficient permissions.", null))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
