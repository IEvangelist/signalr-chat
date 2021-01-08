using BlazoR.Chat.Server.Extensions;
using BlazorR.Chat.Hubs;
using BlazorR.Chat.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net.Mime;

namespace BlazoR.Chat.Server
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddResponseCompression(
                options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { MediaTypeNames.Application.Octet }));

            services.AddAppAuthentication(_configuration);
            services.AddAppServices(_configuration);

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.Configure<TranslatorTextOptions>(
                _configuration.GetSection(nameof(TranslatorTextOptions)));

            services.AddSignalR(options => options.EnableDetailedErrors = true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(policy =>
                policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
                    .AllowAnyMethod()
                    .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization)
                    .AllowCredentials());
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                endpoints.MapHub<ChatHub>("/chat");
            });
        }
    }
}
