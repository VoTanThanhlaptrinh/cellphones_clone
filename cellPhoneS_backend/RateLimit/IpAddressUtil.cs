namespace cellPhoneS_backend.RateLimit
{
    public static class IpAddressUtil
    {
        public static string GetClientIpAddress(HttpContext context)
        {
            // Priority 1: Check X-Forwarded-For header (used by proxies like Render, Cloudflare, etc.)
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    // Take the first IP (the original client IP)
                    return ips[0].Trim();
                }
            }

            // Priority 2: Fall back to the direct connection's remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
