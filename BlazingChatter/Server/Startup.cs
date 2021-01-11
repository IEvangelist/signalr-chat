using BlazingChatter.Hubs;
using BlazingChatter.Options;
using BlazingChatter.Server.Data;
using BlazingChatter.Server.Extensions;
using BlazingChatter.Server.Models;
using BlazingChatter.Server.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace BlazingChatter.Server
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(
                options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { MediaTypeNames.Application.Octet }));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    _configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton
                    <IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>());

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
                app.UseMigrationsEndPoint();
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

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //static Task RegisterTask(HttpContext context) =>
                //    Task.Factory.StartNew(
                //        () => context.Response.Redirect("/Identity/Account/Login", true, true));

                //endpoints.MapGet("/Identity/Account/Register", RegisterTask);
                //endpoints.MapPost("/Identity/Account/Register", RegisterTask);

                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chat");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
