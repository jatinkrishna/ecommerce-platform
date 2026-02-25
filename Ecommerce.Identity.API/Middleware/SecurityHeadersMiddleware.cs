namespace Ecommerce.Identity.API.Middleware
{
    /// <summary>
    /// Middleware to add security headers to all responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Remove server identification headers
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");

            // Prevent MIME type sniffing
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // Prevent clickjacking
            context.Response.Headers.Add("X-Frame-Options", "DENY");

            // XSS protection (legacy but still useful)
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Control referrer information
            context.Response.Headers.Add("Referrer-Policy", "no-referrer");

            // Restrict permissions
            context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
            
            // HTTP Strict Transport Security (only on HTTPS)
            if (context.Request.IsHttps)
            {
                context.Response.Headers.Add("Strict-Transport-Security", 
                    "max-age=31536000; includeSubDomains; preload");
            }

            // Content Security Policy
            context.Response.Headers.Add("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' data:; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self'");

            _logger.LogDebug("Security headers added to response for {Path}", context.Request.Path);
            
            await _next(context);
        }
    }
}
