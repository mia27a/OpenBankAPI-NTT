using System.Net;
using System.Text.Json;

namespace APIBank.Helpers
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception error)
            {
                httpContext.Response.ContentType = "application/json";

                switch (error)
                {
                    case AppException:
                        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest; // custom application error
                        break;
                    case KeyNotFoundException:
                        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound; // not found error
                        break;
                    default:
                        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // unhandled error
                        break;
                }

                _logger.LogInformation("Global Error was found:");
                var result = JsonSerializer.Serialize(new { message = error?.Message });
                await httpContext.Response.WriteAsync(result);
            }
        }
    }
}