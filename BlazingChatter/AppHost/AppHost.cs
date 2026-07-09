var builder = DistributedApplication.CreateBuilder(args);

// Pure API + SignalR backend (joke bots, translation, the /chat hub).
var api = builder.AddProject<Projects.BlazingChatter_Server>("api");

// Standalone Blazor WebAssembly client, wired to the backend API via service discovery.
var web = builder.AddBlazorWasmProject<Projects.BlazingChatter_Client>("web")
    .WithReference(api);

// Browser-facing gateway that serves the WASM app and fronts the backend.
builder.AddBlazorGateway("gateway")
    .WithExternalHttpEndpoints()
    .WithBlazorClientApp(web);

builder.Build().Run();
