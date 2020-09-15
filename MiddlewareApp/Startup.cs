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
using Microsoft.Extensions.Logging;
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            var _logger = loggerFactory.CreateLogger("PipelineLogger");
            _logger.LogInformation("Pipeline started");

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
            app.Map("/welcome", HandleMapWelcome);
           
            app.UseRouting();
            //app.UseAuthorization();
            
            app.UseLoggerMiddleware();
            app.Use(async (context, next) => {
                context.Response.Headers.Append("Content-Type", "text/html");
                await next();
            });
            app.Map("/hello", HandleMapHello);
            app.MapWhen(context => context.Request.Query.ContainsKey("company"), HandleMapWork);
            app.Use(async (context, next) => {
               
                if (context.Request.Path.StartsWithSegments("/addHeader"))
                {
                    _logger.LogInformation("AddHeader called");
                    context.Response.Headers.Append("CustomHeader", "Test");
                }
                await next();
                _logger.LogInformation("Response arrived back in addHeader section");
            });
            app.UseWhen(context => context.Response.Headers.ContainsKey("CustomHeader"), HandleSecHeader);
            app.Use(async (context, next) =>
            {
                //System.Threading.Thread.Sleep(5000);

                
                if (context.Response.Headers.ContainsKey("second-header"))
                {
                    _logger.LogInformation("Response contains key second-header");
                    var keyVal = context.Response.Headers["second-header"].ToString();
                    await context.Response.WriteAsync($"<h2>Value from header: {keyVal}</h2>");
                }
                else if (context.Request.Headers.ContainsKey("no-second")){
                    throw new Exception("Header no-second not found");
                }
                else { await next(); }
                

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
                await context.Response.WriteAsync("<h2> Hello from hello branch </h2>");
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
            app.Use(async (context, next) => {
                if (!context.Request.Headers.ContainsKey("no-second"))
                {
                   context.Response.Headers.Append("second-header", "header-test");
                    await next();
                }
                else
                {
                    await next();
                }
            });
        }
       
        private static void HandleMapWelcome(IApplicationBuilder app)
        {
            app.UseWelcomePage();
        }
    }
}
