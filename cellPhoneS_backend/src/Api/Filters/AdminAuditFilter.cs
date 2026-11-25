using System.Text.Json;
using System.Text.Json.Nodes;
using cellphones_backend.Models;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace cellPhoneS_backend.Filters
{
    /// <summary>
    /// Ghi <see cref="AuditLog"/> cho mọi request ghi (POST/PUT/PATCH/DELETE) dưới <c>/api/admin/</c>, sau khi action chạy để có status code.
    /// Lỗi khi ghi chỉ được log, không làm hỏng request. Ghi bằng scope/DbContext riêng để không dính state của request.
    /// Không ghi được request bị chặn trước action (401/403 từ middleware hoặc [RequirePermission]).
    /// </summary>
    public class AdminAuditFilter : IAsyncActionFilter, IOrderedFilter
    {
        private const int MaxPayloadLength = 4000;
        private static readonly string[] SensitiveKeys = { "password", "token", "secret" };
        private static readonly JsonSerializerOptions JsonOptions = new() { MaxDepth = 16 };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AdminAuditFilter> _logger;

        public AdminAuditFilter(IServiceScopeFactory scopeFactory, ILogger<AdminAuditFilter> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // Chạy ngoài cùng (trước ValidationActionFilter) để thấy cả request bị chặn bởi validation (400).
        public int Order => -3000;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            if (!IsAuditable(request))
            {
                await next();
                return;
            }

            var executed = await next();

            try
            {
                var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return;

                var path = request.Path.Value ?? "";
                var (resource, resourceId) = ParseResource(path);
                var entry = new AuditLog
                {
                    UserId = userId,
                    Method = request.Method.ToUpperInvariant(),
                    Path = Truncate(path, 256)!,
                    Resource = Truncate(resource, 64),
                    ResourceId = Truncate(resourceId, 64),
                    StatusCode = ResolveStatusCode(executed),
                    Payload = BuildPayload(context.ActionArguments),
                    Ip = Truncate(context.HttpContext.Connection.RemoteIpAddress?.ToString(), 64)
                };

                using var scope = _scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IAuditLogService>().Write(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không ghi được audit log cho {Method} {Path}", request.Method, request.Path);
            }
        }

        private static bool IsAuditable(HttpRequest request)
            => request.Path.StartsWithSegments("/api/admin", StringComparison.OrdinalIgnoreCase)
               && !HttpMethods.IsGet(request.Method)
               && !HttpMethods.IsHead(request.Method)
               && !HttpMethods.IsOptions(request.Method);

        private static int ResolveStatusCode(ActionExecutedContext executed)
        {
            if (executed.Exception != null && !executed.ExceptionHandled)
                return ApiExceptionFilter.MapStatusCode(executed.Exception) ?? StatusCodes.Status500InternalServerError;
            return (executed.Result as Microsoft.AspNetCore.Mvc.Infrastructure.IStatusCodeActionResult)?.StatusCode ?? StatusCodes.Status200OK;
        }

        /// <summary>"/api/admin/orders/15/status" → ("orders", "15"). Chỉ coi segment thứ hai là id khi là số hoặc GUID.</summary>
        private static (string? resource, string? resourceId) ParseResource(string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // segments: api, admin, <resource>, <id>?, ...
            var resource = segments.Length > 2 ? segments[2].ToLowerInvariant() : null;
            string? resourceId = null;
            if (segments.Length > 3 && (long.TryParse(segments[3], out _) || Guid.TryParse(segments[3], out _)))
                resourceId = segments[3];
            return (resource, resourceId);
        }

        private static string? BuildPayload(IDictionary<string, object?> arguments)
        {
            var root = new JsonObject();
            foreach (var (name, value) in arguments)
            {
                if (value is null or IFormFile or IFormFileCollection or CancellationToken) continue;
                try
                {
                    root[name] = JsonSerializer.SerializeToNode(value, value.GetType(), JsonOptions);
                }
                catch (Exception)
                {
                    // Argument không serialize được (stream, vòng lặp...): bỏ qua, không làm mất cả dòng audit.
                }
            }
            if (root.Count == 0) return null;

            Redact(root);
            return Truncate(root.ToJsonString(), MaxPayloadLength);
        }

        private static void Redact(JsonNode? node)
        {
            switch (node)
            {
                case JsonObject obj:
                    foreach (var key in obj.Select(p => p.Key).ToList())
                    {
                        if (SensitiveKeys.Any(s => key.Contains(s, StringComparison.OrdinalIgnoreCase)))
                            obj[key] = "***";
                        else
                            Redact(obj[key]);
                    }
                    break;
                case JsonArray array:
                    foreach (var item in array) Redact(item);
                    break;
            }
        }

        private static string? Truncate(string? value, int max)
            => value is null || value.Length <= max ? value : value[..max];
    }
}
