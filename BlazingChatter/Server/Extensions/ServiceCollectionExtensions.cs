using BlazingChatter.Bots;
using BlazingChatter.Factories;
using BlazingChatter.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace BlazingChatter.Server.Extensions
{
    static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddAppAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAdB2C"));

            services.Configure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters.NameClaimType = "name";
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
