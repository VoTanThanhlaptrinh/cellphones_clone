namespace cellPhoneS_backend.RateLimit
{
    /// Central configuration for rate limiting rules.
    public static class RateLimitConfig
    {
        /// Returns the list of rate limiting rules.
        /// Rules are evaluated in order - first match wins.
        public static List<RateLimitRule> GetRules()
        {
            return new List<RateLimitRule>
            {
                // SENSITIVE ENDPOINTS (5 req/min, IP blocked on violation)
                // Authentication - Login
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/login",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                
                // Authentication - Registration
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/register",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/studentRegister",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/teacherRegister",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                
                // OAuth2 Authentication
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/Oauth2-google",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/oauth2Zalo",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                
                // Token Refresh
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/refresh",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                
                // Payment Processing - Creating/Processing Payments
                new RateLimitRule
                {
                    UrlPattern = "/api/payments/*",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                new RateLimitRule
                {
                    UrlPattern = "/api/payments/*",
                    Method = "PUT",
                    Policy = RateLimitPolicyType.Sensitive
                },
                
                // Order Creation/Checkout
                new RateLimitRule
                {
                    UrlPattern = "/api/orders/*",
                    Method = "POST",
                    Policy = RateLimitPolicyType.Sensitive
                },
                
                // ========================================
                // AUTHENTICATED ENDPOINTS (120 req/min)
                // ========================================
                
                // Admin Routes - All operations
                new RateLimitRule
                {
                    UrlPattern = "/api/admin/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Authenticated
                },
                
                // Authentication - Logout
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/logout",
                    Method = null,
                    Policy = RateLimitPolicyType.Authenticated
                },
                
                // Cart Management
                new RateLimitRule
                {
                    UrlPattern = "/api/carts/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Authenticated
                },
                
                // Order Management - Viewing/Updating (excluding POST handled above)
                new RateLimitRule
                {
                    UrlPattern = "/api/orders/*",
                    Method = "GET",
                    Policy = RateLimitPolicyType.Authenticated
                },
                new RateLimitRule
                {
                    UrlPattern = "/api/orders/*",
                    Method = "PUT",
                    Policy = RateLimitPolicyType.Authenticated
                },
                new RateLimitRule
                {
                    UrlPattern = "/api/orders/*",
                    Method = "PATCH",
                    Policy = RateLimitPolicyType.Authenticated
                },
                new RateLimitRule
                {
                    UrlPattern = "/api/orders/*",
                    Method = "DELETE",
                    Policy = RateLimitPolicyType.Authenticated
                },
                
                // Payment History - Viewing (excluding POST/PUT handled above)
                new RateLimitRule
                {
                    UrlPattern = "/api/payments/*",
                    Method = "GET",
                    Policy = RateLimitPolicyType.Authenticated
                },
                new RateLimitRule
                {
                    UrlPattern = "/api/payments/*",
                    Method = "DELETE",
                    Policy = RateLimitPolicyType.Authenticated
                },
                
                // User Profile Management
                new RateLimitRule
                {
                    UrlPattern = "/api/user/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Authenticated
                },
                
                // ========================================
                // PUBLIC ENDPOINTS (60 req/min)
                // ========================================
                
                // OAuth Callback
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/callBack/google",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Auth Status Check
                new RateLimitRule
                {
                    UrlPattern = "/api/auth/isLoggedIn",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Home Page
                new RateLimitRule
                {
                    UrlPattern = "/api/home",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Health Check
                new RateLimitRule
                {
                    UrlPattern = "/api/health/healthcheck",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Static Images
                new RateLimitRule
                {
                    UrlPattern = "/api/img/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Product Listings
                new RateLimitRule
                {
                    UrlPattern = "/api/products/list/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Products by Brand
                new RateLimitRule
                {
                    UrlPattern = "/api/products/brand/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Products by Series
                new RateLimitRule
                {
                    UrlPattern = "/api/products/series/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Products by Category
                new RateLimitRule
                {
                    UrlPattern = "/api/products/category/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Product Details - GET only (modifications would be admin)
                new RateLimitRule
                {
                    UrlPattern = "/api/products/*",
                    Method = "GET",
                    Policy = RateLimitPolicyType.Public
                },
                
                // Category Browsing
                new RateLimitRule
                {
                    UrlPattern = "/api/category/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // Search Functionality
                new RateLimitRule
                {
                    UrlPattern = "/api/search",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                },
                
                // ========================================
                // FALLBACK RULES
                // ========================================
                
                // Catch-all for any other API routes - default to public
                new RateLimitRule
                {
                    UrlPattern = "/api/*",
                    Method = null,
                    Policy = RateLimitPolicyType.Public
                }
            };
        }
    }
}
