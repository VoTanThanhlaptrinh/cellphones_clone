using System.Net;
using System.Security.Claims;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Auth
{
    public class CentralizedAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly List<RoutePolicy> _policies;

        public CentralizedAuthMiddleware(RequestDelegate next, List<RoutePolicy> policies)
        {
            _next = next;
            _policies = policies;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method.ToUpper();

            if (string.IsNullOrEmpty(path))
            {
                await _next(context);
                return;
            }
            var policy = _policies.FirstOrDefault(p => p.IsMatch(path, method));

            if (policy != null)
            {
                if (policy.AllowAnonymous)
                {
                    await _next(context);
                    return;
                }

                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(new ApiResponse<object?>("Unauthorized: Authentication required.", null));
                    return;
                }

                if (policy.RequiredRoles.Count() > 0)
                {
                    var hasRole = policy.RequiredRoles.Any(role => context.User.IsInRole(role));
                    if (!hasRole)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsJsonAsync(new ApiResponse<object?>("Forbidden: Insufficient permissions.", null));
                        return;
                    }
                }

                if (policy.RequiredPermissions.Count > 0)
                {
                    // Middleware được tạo một lần cho cả app: service scoped phải lấy từ RequestServices.
                    var permissionService = context.RequestServices.GetRequiredService<IPermissionService>();
                    var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
                    var granted = await permissionService.GetPermissionsAsync(roles);
                    if (!policy.RequiredPermissions.All(granted.Contains))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsJsonAsync(new ApiResponse<object?>("Forbidden: Insufficient permissions.", null));
                        return;
                    }
                }
            }
            await _next(context);
        }
    }
}
