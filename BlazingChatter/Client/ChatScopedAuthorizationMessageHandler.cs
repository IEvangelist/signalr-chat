using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazingChatter.Client
{
    public class ChatScopedAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        const string scope =
            "https://dotnetdocs.onmicrosoft.com/50e82891-dead-4d8c-b301-a70ec41a8528/user_chat";

        public ChatScopedAuthorizationMessageHandler(
            IAccessTokenProvider provider,
            NavigationManager navigationManager) : base(provider, navigationManager) =>
            ConfigureHandler(
                authorizedUrls: new[]
                {
                    "http://localhost:5001",
                    "https://dotnetdocs.b2clogin.com"
                },
                scopes: new[] { scope });
    }
}