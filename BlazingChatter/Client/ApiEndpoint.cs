namespace BlazingChatter.Client;

/// <summary>
/// Holds the resolved base address of the backend <c>api</c> resource.
/// </summary>
/// <remarks>
/// When the app runs behind the Aspire Blazor gateway it is served under a path
/// prefix (for example <c>/web</c>) and the API is reached through a reverse-proxied
/// route (<c>/web/_api/api</c>). The gateway publishes that address via its
/// <c>_blazor/_configuration</c> endpoint. For standalone runs this falls back to the
/// host's own base address.
/// </remarks>
public sealed class ApiEndpoint(string baseAddress)
{
    /// <summary>The API base address, always terminated with a trailing slash.</summary>
    public Uri BaseAddress { get; } =
        new(baseAddress.EndsWith('/') ? baseAddress : baseAddress + "/");

    /// <summary>The absolute URI of the SignalR chat hub (<c>{apiBase}/chat</c>).</summary>
    public Uri HubUri => new(BaseAddress, "chat");
}
