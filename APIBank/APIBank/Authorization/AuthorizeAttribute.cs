using Microsoft.AspNetCore.Mvc.Filters;

namespace APIBank.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            // skip if AllowAnonymous attribute is used
            var allowAnonymous = filterContext.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
            {
                return;
            }


            // authorization
            User user = (User)filterContext.HttpContext.Items["User"];
            if (user == null)
            {
                filterContext.Result = new JsonResult(new { message = "Unauthorized - User unable to perform this action." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

        }
    }
}