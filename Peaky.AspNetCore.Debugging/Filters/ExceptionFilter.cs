using Microsoft.AspNetCore.Mvc.Filters;
using Peaky.AspNetCore.Debugging.Static;

namespace Peaky.AspNetCore.Debugging.Filters
{
    internal class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext ctx)
        {
            // Stores the exception in the HttpContext item bag for later usage in DebugMiddleware
            ctx.HttpContext.Items[PeakyDebugConstants.HttpContextDebugItem] = ctx.Exception;
        }
    }
}
