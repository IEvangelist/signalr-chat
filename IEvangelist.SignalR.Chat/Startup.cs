using IEvangelist.SignalR.Chat.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IEvangelist.SignalR.Chat
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.ConfigureCookiePolicy();
            services.AddChatAuthentication(_configuration);
            services.AddChatServices();

            services.AddSignalR(options => options.EnableDetailedErrors = true)
                    .AddAzureSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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
               // Gone with .NET Core 3.0
               .UseAzureSignalR(routes => routes.MapHub<ChatHub>("/chat"))
               .UseMvc();
        }
    }
}