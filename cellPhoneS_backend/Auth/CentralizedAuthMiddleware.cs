using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
                    await context.Response.WriteAsJsonAsync(new { message = "Unauthorized: Authentication required." });
                    return;
                }

                if (policy.RequiredRoles.Any())
                {
                    var hasRole = policy.RequiredRoles.Any(role => context.User.IsInRole(role));
                    if (!hasRole)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsJsonAsync(new { message = "Forbidden: Insufficient permissions." });
                        return;
                    }
                }
            }
            await _next(context);
        }
    }
}
