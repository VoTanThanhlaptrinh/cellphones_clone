namespace cellPhoneS_backend.RateLimit
{
    /// Represents a rate limiting rule that maps URL patterns to policies.
    public class RateLimitRule
    {
        /// URL pattern to match. Supports wildcards (e.g., "/api/products/*").
        public string UrlPattern { get; set; } = string.Empty;
        /// HTTP method to match (GET, POST, etc.). Null means all methods.
        public string? Method { get; set; }
        /// The rate limiting policy to apply when this rule matches.
        public RateLimitPolicyType Policy { get; set; }
    }
}
