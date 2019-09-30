using Microsoft.AspNetCore.Mvc.Filters;

namespace Peaky.Extensions.Web.Debug
{
    internal class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext ctx)
        {
            // Stores the exception in the HttpContext item bag for later usage in DebugMiddleware
            ctx.HttpContext.Items[Constants.HttpContextDebugItem] = ctx.Exception;
        }
    }
}
