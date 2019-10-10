using IEvangelist.SignalR.Chat.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IEvangelist.SignalR.Chat
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.ConfigureCookiePolicy();
            services.AddChatAuthentication(_configuration);
            services.AddChatServices();

            services.AddSignalR(options => options.EnableDetailedErrors = true)
                    .AddAzureSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection()
               .UseStaticFiles()
               .UseCookiePolicy()
               .UseAuthentication()
               .UseAuthorization()
               .UseRouting()
               .UseEndpoints(routes => 
               {
                   routes.MapRazorPages();
                   routes.MapHub<ChatHub>("/chat");
               });
        }
    }
}