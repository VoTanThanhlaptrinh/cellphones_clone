namespace cellPhoneS_backend.RateLimit
{
    public enum RateLimitPolicyType
    {
        /// Sensitive endpoints: 5 requests per minute. IP is blocked on violation.
        Sensitive,
        /// Public endpoints: 60 requests per minute.
        Public,
        /// Authenticated endpoints: 120 requests per minute.
        Authenticated
    }
}
