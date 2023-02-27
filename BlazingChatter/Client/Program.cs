using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using BlazingChatter.Client;

const string api_scope =
    "https://dotnetdocs.onmicrosoft.com/50e82891-dead-4d8c-b301-a70ec41a8528/user_chat";

const string ServerApi = nameof(ServerApi);

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddHttpClient(ServerApi,
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddScoped(
    sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(ServerApi));

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);

    options.ProviderOptions.DefaultAccessTokenScopes.Add(api_scope);
    options.ProviderOptions.LoginMode = "redirect";
});

builder.Services.AddLocalStorageServices();
builder.Services.AddSpeechSynthesisServices();

await builder.Build().RunAsync();