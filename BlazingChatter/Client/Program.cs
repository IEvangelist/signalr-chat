using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using BlazingChatter.Client;
using BlazingChatter.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Resolve the backend "api" base address. Behind the Aspire Blazor gateway the app is
// served under a path prefix and the API is reached through a reverse-proxied route; the
// gateway advertises the resolved address via its "_blazor/_configuration" endpoint. If
// that lookup fails (e.g. a standalone run) fall back to the host's own base address.
var apiBaseAddress =
    await ResolveApiBaseAddressAsync(builder.HostEnvironment.BaseAddress)
    ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddSingleton(new ApiEndpoint(apiBaseAddress));

// OpenID Connect against the self-contained Keycloak realm. The authority/client id come
// from configuration (wwwroot/appsettings.json). Authorization code flow with PKCE is used
// (public client), and "preferred_username" is projected as the user's name so it matches
// the server's identity - keeping message ownership checks consistent across client/server.
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Oidc", options.ProviderOptions);

    options.ProviderOptions.ResponseType = "code";
    options.UserOptions.NameClaim = "preferred_username";
});

builder.Services.AddLocalStorageServices();
builder.Services.AddSpeechSynthesisServices();
builder.Services.AddSingleton<IToastService, ToastService>();

await builder.Build().RunAsync();

static async Task<string?> ResolveApiBaseAddressAsync(string hostBaseAddress)
{
    try
    {
        using var http = new HttpClient { BaseAddress = new Uri(hostBaseAddress) };

        var config = await http.GetFromJsonAsync<GatewayConfiguration>(
            "_blazor/_configuration");

        var environment = config?.WebAssembly?.Environment;
        if (environment is null)
        {
            return null;
        }

        return environment.GetValueOrDefault("services__api__https__0")
            ?? environment.GetValueOrDefault("services__api__http__0");
    }
    catch (HttpRequestException)
    {
        return null;
    }
    catch (NotSupportedException)
    {
        return null;
    }
}

sealed record GatewayConfiguration(GatewayWebAssemblyConfiguration? WebAssembly);

sealed record GatewayWebAssemblyConfiguration(Dictionary<string, string>? Environment);