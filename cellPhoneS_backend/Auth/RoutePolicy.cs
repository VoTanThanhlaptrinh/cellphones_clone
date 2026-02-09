using System.Text.RegularExpressions;

namespace cellPhoneS_backend.Auth
{
    public class RoutePolicy
    {
        public string UrlPattern { get; set; } = string.Empty;
        public string Method { get; set; } = "*"; // "GET", "POST", etc., or "*" for all
        public bool AllowAnonymous { get; set; } = false;
        public List<string> RequiredRoles { get; set; } = new List<string>();

        // Precompiled regex for performance if needed, or simple prefix match
        // For Spring Security AntPathMatcher style, meaningful implementation is needed.
        // Here we will support:
        // 1. Exact match: "/api/auth/login"
        // 2. Prefix match (wildcard at end): "/api/products/*"
        
        public bool IsMatch(string requestPath, string requestMethod)
        {
            if (Method != "*" && !Method.Equals(requestMethod, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (UrlPattern.EndsWith("/*"))
            {
                var prefix = UrlPattern.Substring(0, UrlPattern.Length - 2);
                return requestPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            return requestPath.Equals(UrlPattern, StringComparison.OrdinalIgnoreCase);
        }
    }
}
