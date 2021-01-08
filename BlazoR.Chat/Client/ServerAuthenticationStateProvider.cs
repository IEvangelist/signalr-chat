using BlazorR.Chat.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazoR.Chat.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        readonly HttpClient _httpClient;

        public ServerAuthenticationStateProvider(HttpClient httpClient) => _httpClient = httpClient;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userInfoJson = await _httpClient.GetStringAsync("user");
            var userInfo = userInfoJson.FromJson<UserInfo>();
            var identity = userInfo.IsAuthenticated
                ? new ClaimsIdentity(new Claim[] { new(ClaimTypes.Name, userInfo.Name) }, "serverauth")
                : new ClaimsIdentity();

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
