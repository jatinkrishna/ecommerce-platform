using Ecommerce.Shared.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace Ecommerce.Identity.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            context.Response.ContentType = "application/json";

            int statusCode;
            string message;

            switch (exception)
            {
                case UnauthorizedException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = exception.Message;
                    break;

                case NotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = exception.Message;
                    break;

                case ConflictException:
                    statusCode = StatusCodes.Status409Conflict;
                    message = exception.Message;
                    break;

                case ApiException apiException:
                    statusCode = apiException.StatusCode;
                    message = apiException.Message;
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "An internal server error occurred";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                message = message,
                statusCode = statusCode
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
