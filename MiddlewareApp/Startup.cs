using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiddlewareApp.Middleware;

namespace MiddlewareApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
       
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseWelcomePage();
            app.UseRouting();
            //app.UseAuthorization();
            
            app.UseLoggerMiddleware();

            app.Map("/hello", HandleMapHello);
            app.MapWhen(context => context.Request.Query.ContainsKey("company"), HandleMapWork);
            app.Use(async (context, next) => {
                if (context.Request.Path.StartsWithSegments("/addHeader"))
                {
                    context.Response.Headers.Append("CustomHeader", "Test");
                }
                await next();
            });
            app.UseWhen(context => context.Response.Headers.ContainsKey("CustomHeader"), HandleSecHeader);
            app.Use(async (context, next) =>
            {
                //System.Threading.Thread.Sleep(5000);

                //throw new Exception("Error Occurred while processing your request");
                //await context.Response.WriteAsync("<h1> Hello from custom middleware !</h1>");
                if(context.Response.Headers.ContainsKey("second-header"))
                {
                    context.Response.Headers.TryGetValue("second-header", out var keyVal);
                    await context.Response.WriteAsync($"<h2>Value from header: {keyVal}");
                }
                else {
                    await next();
                    await context.Response.WriteAsync("<h3>No header detected</h3>");
                }
                
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }

        private static void HandleMapHello(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("<h3>Hello from hello branch</h3>");
            });
        }

        private static void HandleMapWork(IApplicationBuilder app)
        {
            app.Run(async context => {
                var company = context.Request.Query["company"];
                await context.Response.WriteAsync($"<h1>Company = {company}</h1>");
            });
        }

        private static void HandleSecHeader(IApplicationBuilder app)
        {
            app.Run(async context => context.Response.Headers.Append("second-header", "header-test"));
        }
       
    }
}
