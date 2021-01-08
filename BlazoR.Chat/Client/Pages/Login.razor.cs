using BlazoR.Chat.Shared;
using BlazorR.Chat.Extensions;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazoR.Chat.Client.Pages
{
    public partial class Login
    {
        [Inject] public HttpClient Http { get; set; }

        private List<AuthScheme> _authSchemes;

        protected override async Task OnInitializedAsync()
        {
            var schemesJson = await Http.GetStringAsync("user/schemes");
            _authSchemes = schemesJson.FromJson<List<AuthScheme>>();
        }

        public async Task SignInAsync(string scheme)
        {
            await Http.GetAsync($"user/signin?scheme={scheme}");
        }
    }
}
