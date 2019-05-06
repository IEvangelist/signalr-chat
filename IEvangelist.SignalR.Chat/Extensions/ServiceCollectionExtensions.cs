using IEvangelist.SignalR.Chat.Bots;
using IEvangelist.SignalR.Chat.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureCookiePolicy(
            this IServiceCollection services)
            => services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

        public static IServiceCollection AddChatAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            _ = services.AddAuthentication("Cookies")
                        .AddCookie()
                        .AddTwitter(o => configuration.GetSection("Authentication:Twitter").Bind(o))
                        .AddGoogle(o =>
                        {
                            o.ClientId = configuration["Authentication:Google:ClientId"];
                            o.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                            o.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                            o.ClaimActions.Clear();
                            o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                            o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                            o.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                            o.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                            o.ClaimActions.MapJsonKey("urn:google:profile", "link");
                            o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                        });

            return services;
        }

        public static IServiceCollection AddChatServices(
            this IServiceCollection services)
        {
            services.AddSingleton<ICommandSignal, CommandSignal>();
            services.AddHttpClient<IDadJokeService, DadJokeService>(
                client => client.DefaultRequestHeaders.Add("Accept", "text/plain"));
            return services.AddHostedService<ChatBotService>();
        }
    }
}