using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MiddlewareApp.Middleware
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public LoggerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger("LoggerMiddleware");
        }

        public async Task Invoke(HttpContext httpContext)
        {
            _logger.LogInformation("Incoming request");
            await _next(httpContext);
            _logger.LogInformation("Outgoing response");
        }
    }

    public static class AppBuilderExtensions
    {
        public static void UseLoggerMiddleware(this IApplicationBuilder app){
            app.UseMiddleware<LoggerMiddleware>();
        }
    }
}
