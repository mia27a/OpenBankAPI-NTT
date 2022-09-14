using Microsoft.Extensions.Options;

namespace APIBank.Authorization
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next/*, IOptions<AppSettings> appSettings*/)
        {
            _next = next;
            //_appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext httpContext, IUserService userService, IJwtUtils jwtUtils)
        {
            var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtUtils.ValidateJWTToken(token!);
            if (userId != null)
            {
                // attach user to context on successful jwt validation
                httpContext.Items["User"] = userService.GetById(userId.Value);
            }

            await _next(httpContext);
        }
    }
}