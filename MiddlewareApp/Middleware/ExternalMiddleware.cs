using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MiddlewareApp.Middleware
{
    public class ExternalMiddleware
    {
        private readonly RequestDelegate _next;
        public ExternalMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next(httpContext);
           
            await httpContext.Response.WriteAsync("<h5>Hello from external middleware</h5>");
        }
    }

    public static class AppBuilderExtensions
    {
        public static void UseExternalMiddleware(this IApplicationBuilder app){
            app.UseMiddleware<ExternalMiddleware>();
        }
    }
}
