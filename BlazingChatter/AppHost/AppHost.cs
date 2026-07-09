var builder = DistributedApplication.CreateBuilder(args);

// Self-contained Keycloak identity provider. A fixed host port keeps the authority URL
// (and the OIDC tokens that embed it) stable across AppHost restarts. Aspire serves Keycloak
// over HTTPS with the trusted localhost dev certificate, so the browser reaches it directly
// at https://localhost:8080 without CORS or certificate friction. The realm import seeds a
// realm, a public PKCE web client, and a dev test user so login works out of the box.
var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithRealmImport("./Realms");

// Pure API + SignalR backend (joke bots, translation, the /chat hub). Validates the Keycloak
// access tokens and waits for Keycloak to be ready before starting.
var api = builder.AddProject<Projects.BlazingChatter_Server>("api")
    .WithReference(keycloak)
    .WaitFor(keycloak);

// Standalone Blazor WebAssembly client, wired to the backend API via service discovery.
// The browser talks to Keycloak directly (fixed port), so no keycloak reference is needed
// here - that also keeps the token issuer as the direct http://localhost:8080 authority.
var web = builder.AddBlazorWasmProject<Projects.BlazingChatter_Client>("web")
    .WithReference(api);

// Browser-facing gateway that serves the WASM app and fronts the backend. Its endpoints are
// pinned to stable ports so the OIDC redirect URIs registered in the Keycloak realm remain
// deterministic (Keycloak redirect-URI wildcards cover the path, not the host:port).
builder.AddBlazorGateway("gateway")
    .WithExternalHttpEndpoints()
    .WithBlazorClientApp(web)
    .WithEndpoint("https", endpoint => endpoint.Port = 7443)
    .WithEndpoint("http", endpoint => endpoint.Port = 7080);

builder.Build().Run();
