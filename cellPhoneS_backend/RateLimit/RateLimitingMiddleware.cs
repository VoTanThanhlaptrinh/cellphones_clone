using StackExchange.Redis;

namespace cellPhoneS_backend.RateLimit
{
    /// Middleware for centralized rate limiting using Redis.
    /// Designed to work behind proxies (e.g., Render) by reading X-Forwarded-For headers.
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConnectionMultiplexer _redis;
        private readonly List<RateLimitRule> _rules;

        public RateLimitingMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
        {
            _next = next;
            _redis = redis;
            _rules = RateLimitConfig.GetRules();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract client IP address (handles proxy scenarios)
            var clientIp = IpAddressUtil.GetClientIpAddress(context);
            var path = context.Request.Path.Value ?? "";
            var method = context.Request.Method;

            // Match the request against configured rules
            var matchedRule = MatchRule(path, method);

            if (matchedRule != null)
            {
                // TODO: Check if the IP is in the Redis blocklist
                // var db = _redis.GetDatabase();
                // var blockKey = $"ratelimit:block:{clientIp}";
                // var isBlocked = await db.KeyExistsAsync(blockKey);
                // if (isBlocked)
                // {
                //     context.Response.StatusCode = 403;
                //     await context.Response.WriteAsync("IP address is blocked due to rate limit violations");
                //     return;
                // }

                // TODO: Increment the request counter in Redis with a 1-minute TTL
                // var counterKey = $"ratelimit:{matchedRule.Policy}:{clientIp}";
                // var requestCount = await db.StringIncrementAsync(counterKey);
                // if (requestCount == 1)
                // {
                //     await db.KeyExpireAsync(counterKey, TimeSpan.FromMinutes(1));
                // }

                // TODO: Check if the limit is exceeded and apply penalties
                // var limit = GetLimitForPolicy(matchedRule.Policy);
                // if (requestCount > limit)
                // {
                //     if (matchedRule.Policy == RateLimitPolicyType.Sensitive)
                //     {
                //         // Block IP for 15 minutes on sensitive endpoint violation
                //         await db.StringSetAsync(blockKey, "1", TimeSpan.FromMinutes(15));
                //         context.Response.StatusCode = 403;
                //         await context.Response.WriteAsync("IP address blocked due to rate limit violation on sensitive endpoint");
                //     }
                //     else
                //     {
                //         // Return 429 Too Many Requests for non-sensitive endpoints
                //         context.Response.StatusCode = 429;
                //         await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                //     }
                //     return;
                // }
            }

            // Continue to the next middleware
            await _next(context);
        }

        /// Matches the incoming request against configured rate limiting rules.
        /// Returns the first matching rule or null if no match is found.
        private RateLimitRule? MatchRule(string path, string method)
        {
            foreach (var rule in _rules)
            {
                // Check if method matches (if specified in rule)
                if (rule.Method != null && !string.Equals(rule.Method, method, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Check if URL pattern matches
                if (IsPatternMatch(rule.UrlPattern, path))
                {
                    return rule;
                }
            }

            return null;
        }
        /// Checks if a URL pattern matches the given path.
        /// Supports wildcards (e.g., "/api/products/*" matches "/api/products/123").
        private bool IsPatternMatch(string pattern, string path)
        {
            // Handle wildcard pattern (e.g., "/api/products/*")
            if (pattern.EndsWith("/*"))
            {
                var prefix = pattern.Substring(0, pattern.Length - 2);
                return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            // Exact match
            return string.Equals(pattern, path, StringComparison.OrdinalIgnoreCase);
        }

        /// Returns the request limit per minute for a given policy type.
        private int GetLimitForPolicy(RateLimitPolicyType policy)
        {
            return policy switch
            {
                RateLimitPolicyType.Sensitive => 5,        // 5 requests per minute
                RateLimitPolicyType.Public => 60,          // 60 requests per minute
                RateLimitPolicyType.Authenticated => 120,  // 120 requests per minute
                _ => 60
            };
        }
    }
}
