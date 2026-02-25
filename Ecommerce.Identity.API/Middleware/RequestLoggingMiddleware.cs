using System.Diagnostics;
using System.Text;

namespace Ecommerce.Identity.API.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests and responses with performance tracking
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for health check and swagger endpoints to reduce noise
            if (context.Request.Path.StartsWithSegments("/api/health") ||
                context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            var requestId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();

            // Log incoming request
            _logger.LogInformation(
                "HTTP {Method} {Path} started. RequestId: {RequestId}, RemoteIP: {RemoteIP}",
                context.Request.Method,
                context.Request.Path,
                requestId,
                context.Connection.RemoteIpAddress
            );

            // Capture request body for POST/PUT (but not sensitive endpoints)
            var requestBody = await ReadRequestBodyAsync(context);
            if (!string.IsNullOrEmpty(requestBody) && !IsSensitiveEndpoint(context.Request.Path))
            {
                _logger.LogDebug("Request Body: {RequestBody}", requestBody);
            }

            // Capture the original response body stream
            var originalResponseBodyStream = context.Response.Body;

            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Execute the next middleware in the pipeline
                await _next(context);

                stopwatch.Stop();

                // Log response details
                _logger.LogInformation(
                    "HTTP {Method} {Path} completed. RequestId: {RequestId}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    requestId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );

                // Log slow requests (>1000ms)
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "SLOW REQUEST: {Method} {Path} took {Duration}ms. RequestId: {RequestId}",
                        context.Request.Method,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds,
                        requestId
                    );
                }

                // Copy the response back to the original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalResponseBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "HTTP {Method} {Path} failed. RequestId: {RequestId}, Duration: {Duration}ms, Error: {Error}",
                    context.Request.Method,
                    context.Request.Path,
                    requestId,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message
                );

                throw;
            }
            finally
            {
                context.Response.Body = originalResponseBodyStream;
            }
        }

        private static async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            if (context.Request.ContentLength == null || context.Request.ContentLength == 0)
            {
                return string.Empty;
            }

            context.Request.EnableBuffering();

            try
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true
                );

                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                return body;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool IsSensitiveEndpoint(PathString path)
        {
            // Don't log request bodies for these endpoints (contain passwords)
            return path.StartsWithSegments("/api/auth/login") ||
                   path.StartsWithSegments("/api/auth/register") ||
                   path.StartsWithSegments("/api/auth/refresh-token");
        }
    }
}
