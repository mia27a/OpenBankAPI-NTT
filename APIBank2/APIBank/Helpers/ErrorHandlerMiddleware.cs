using System.Net;
using System.Text.Json;

namespace APIBank.Helpers
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        //private readonly ILogger _logger;

        public ErrorHandlerMiddleware(RequestDelegate next/*, ILogger<ErrorHandlerMiddleware> logger*/)
        {
            _next = next;
            //_logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                response.StatusCode = error switch
                {
                    AppException => (int)HttpStatusCode.BadRequest,// custom application error

                    KeyNotFoundException => (int)HttpStatusCode.NotFound,// not found error

                    _ => (int)HttpStatusCode.InternalServerError,// unhandled error
                };
                var result = JsonSerializer.Serialize(new { message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}