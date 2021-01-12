using BlazingChatter.Bots;
using BlazingChatter.Factories;
using BlazingChatter.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace BlazingChatter.Server.Extensions
{
    static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddAppAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(
                    options => options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddTwitter(
                    options =>
                    {
                        configuration.GetSection("Authentication:Twitter").Bind(options);
                        options.SaveTokens = true;
                    })
                .AddGoogle(
                    options =>
                    {
                        options.ClientId = configuration["Authentication:Google:ClientId"];
                        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                        options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                        options.ClaimActions.Clear();
                        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                        options.ClaimActions.MapJsonKey("urn:google:profile", "link");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                        options.SaveTokens = true;
                    });

            return services;
        }

        internal static IServiceCollection AddAppServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<ICommandSignalService, CommandSignalService>();
            services.AddTransient<IJokeServiceFactory, JokeServiceFactory>();
            services.AddTransient<IJokeService, DadJokeService>();
            services.AddTransient<IJokeService, ChuckNorrisJokeService>();

            services.AddHttpClient(nameof(DadJokeService),
                client => client.DefaultRequestHeaders.Add("Accept", "text/plain"));

            services.AddHttpClient(nameof(ChuckNorrisJokeService),
                client => client.DefaultRequestHeaders.Add("Accept", "application/json"));

            services.AddHttpClient<ITranslationService, TranslationService>(
                client =>
                {
                    client.BaseAddress = new(configuration["TranslateTextOptions:Endpoint"]);
                    client.DefaultRequestHeaders
                          .Add("Ocp-Apim-Subscription-Key", configuration["TranslateTextOptions:ApiKey"]);
                    client.DefaultRequestHeaders
                          .Add("Ocp-Apim-Subscription-Region", configuration["TranslateTextOptions:Region"]);
                });

            return services.AddHostedService<ChatBot>();
        }
    }
}
