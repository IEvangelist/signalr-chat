using BlazingChatter.Bots;
using BlazingChatter.Factories;
using BlazingChatter.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace BlazingChatter.Server.Extensions;

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

                // SignalR (WebSockets) can't set the Authorization header, so it passes the
                // access token as a query-string parameter. Read it for the chat hub path.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (string.IsNullOrEmpty(accessToken) is false &&
                            path.StartsWithSegments("/chat"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
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
                if (configuration["TranslateTextOptions:Endpoint"] is string endpoint)
                {
                    client.BaseAddress = new(endpoint);
                }
                client.DefaultRequestHeaders
                      .Add("Ocp-Apim-Subscription-Key", configuration["TranslateTextOptions:ApiKey"]);
                client.DefaultRequestHeaders
                      .Add("Ocp-Apim-Subscription-Region", configuration["TranslateTextOptions:Region"]);
            });

        return services.AddHostedService<ChatBot>();
    }
}
