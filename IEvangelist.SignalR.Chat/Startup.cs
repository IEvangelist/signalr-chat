using System.Net;
using System.Security.Claims;
using IEvangelist.SignalR.Chat.Bots;
using IEvangelist.SignalR.Chat.Hubs;
using IEvangelist.SignalR.Chat.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication("Cookies")
                    .AddCookie()
                    .AddTwitter(options => _configuration.GetSection("Authentication:Twitter").Bind(options))
                    .AddGoogle(o =>
                     {
                         o.ClientId = _configuration["Authentication:Google:ClientId"];
                         o.ClientSecret = _configuration["Authentication:Google:ClientSecret"];
                         o.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                         o.ClaimActions.Clear();
                         o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                         o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                         o.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                         o.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                         o.ClaimActions.MapJsonKey("urn:google:profile", "link");
                         o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                     });

            services.AddHostedService<ChatBotService>();
            services.AddSingleton<ICommandSignal, CommandSignal>();
            services.AddHttpClient<IDadJokeService, DadJokeService>(
                client => client.DefaultRequestHeaders.Add("Accept", "text/plain"));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR(options => options.EnableDetailedErrors = true);
                    //.AddAzureSignalR();
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
               .UseSignalR(routes => routes.MapHub<ChatHub>("/chat"))
               //.UseAzureSignalR(routes => routes.MapHub<ChatHub>("/chat"))
               .UseMvc();
        }
    }
}