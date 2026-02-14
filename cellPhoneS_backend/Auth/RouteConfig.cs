using System.Collections.Generic;

namespace cellPhoneS_backend.Auth
{
    public static class RouteConfig
    {
        public static List<RoutePolicy> GetPolicies()
        {
            return new List<RoutePolicy>
            {
                // Public Routes - Authenticated users can also access these, but no login required
                new RoutePolicy { UrlPattern = "/api/auth/login", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/auth/logout", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/auth/refresh", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/auth/register", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/auth/studentRegister", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/auth/teacherRegister", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/auth/Oauth2-google", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/auth/oauth2Zalo", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/auth/callBack/google", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/home", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/health/healthcheck", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/img/*", AllowAnonymous = true }, // Images usually public
                new RoutePolicy { UrlPattern = "/api/auth/isLoggedIn", AllowAnonymous = true },

                // Product Public Read
                new RoutePolicy { UrlPattern = "/api/products/list/*", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/products/brand/*", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/products/series/*", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/products/category/*", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/products/*", Method = "GET", AllowAnonymous = true }, // Detailed view
                
                new RoutePolicy { UrlPattern = "/api/category/*", AllowAnonymous = true },
                new RoutePolicy { UrlPattern = "/api/search", AllowAnonymous = true },

                // Admin Routes - More specific first
                new RoutePolicy { UrlPattern = "/api/admin/*", RequiredRoles = new List<string> { "Admin" } },

                // Authenticated User Routes (and Admin usually)
                // Note: If you want Admin to access User routes, include "Admin" in the roles list
                new RoutePolicy { UrlPattern = "/api/carts/*", RequiredRoles = new List<string> { "User", "Admin" } },
                new RoutePolicy { UrlPattern = "/api/orders/*", RequiredRoles = new List<string> { "User", "Admin" } },
                new RoutePolicy { UrlPattern = "/api/payments/*", RequiredRoles = new List<string> { "User", "Admin" } },
                new RoutePolicy { UrlPattern = "/api/user/*", RequiredRoles = new List<string> { "User", "Admin" } },

                // Catch-all: Require authentication for any other API route
                new RoutePolicy { UrlPattern = "/api/*", RequiredRoles = new List<string>() } 
            };
        }
    }
}
