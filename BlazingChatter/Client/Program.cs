using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using BlazingChatter.Client;
using BlazingChatter.Client.Services;

const string api_scope =
    "https://dotnetdocs.onmicrosoft.com/50e82891-dead-4d8c-b301-a70ec41a8528/user_chat";

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

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);

    options.ProviderOptions.DefaultAccessTokenScopes.Add(api_scope);
    options.ProviderOptions.LoginMode = "redirect";
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