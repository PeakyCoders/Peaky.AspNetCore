using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Peaky.Extensions.Web.Debug
{
    public static class BuilderExtensions
    {
        public static void ConfigureDebugLogging(this IServiceCollection services, Action<MvcOptions> mvcRegisterCallback = null, params string[] highlightedTraces)
        {
            services.AddSingleton(provider => new LoggingMiddleware(highlightedTraces));

            services.AddMvc(options =>
            {
                mvcRegisterCallback?.Invoke(options);

                // Add custom filter to catch exceptions and store them in HttpContext for usage in DebugMiddleware
                options.Filters.Add(new ExceptionFilter());
            });
        }

        public static void UseDebugLogging(this IApplicationBuilder app)
        {
            app.UseMiddleware<LoggingMiddleware>();
        }
    }
}
