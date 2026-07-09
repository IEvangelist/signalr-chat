using BlazingChatter.Bots;
using BlazingChatter.Factories;
using BlazingChatter.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BlazingChatter.Server.Extensions;

static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddAppAuthentication(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        // Validate access tokens issued by the self-contained Keycloak realm. The "keycloak"
        // service name is resolved through Aspire service discovery to the realm's authority
        // (http://localhost:8080/realms/blazingchatter), which is also the issuer the browser
        // obtains tokens from - so issuer validation lines up without extra configuration.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(
                serviceName: "keycloak",
                realm: "blazingchatter",
                options =>
                {
                    // The realm's audience mapper stamps this audience onto access tokens; it is
                    // the Keycloak equivalent of the previous B2C "user_chat" scope gate.
                    options.Audience = "blazingchatter-api";
                    options.TokenValidationParameters.NameClaimType = "preferred_username";

                    // Keycloak is reached over http://localhost:8080 during local development,
                    // so the OIDC metadata is served over HTTP rather than HTTPS.
                    if (environment.IsDevelopment())
                    {
                        options.RequireHttpsMetadata = false;
                    }

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
                // Only configure Azure Translator when an endpoint AND key are supplied.
                // Otherwise the service falls back to the free, key-less MyMemory API
                // (called with absolute URLs), so translation works with no setup.
                var endpoint = configuration["TranslateTextOptions:Endpoint"];
                var apiKey = configuration["TranslateTextOptions:ApiKey"];

                if (!string.IsNullOrWhiteSpace(endpoint) &&
                    !string.IsNullOrWhiteSpace(apiKey))
                {
                    client.BaseAddress = new(endpoint);
                    client.DefaultRequestHeaders
                          .Add("Ocp-Apim-Subscription-Key", apiKey);

                    if (configuration["TranslateTextOptions:Region"] is { Length: > 0 } region)
                    {
                        client.DefaultRequestHeaders
                              .Add("Ocp-Apim-Subscription-Region", region);
                    }
                }
            });

        return services.AddHostedService<ChatBot>();
    }
}
