using System.Net.Http;
using System.Threading.Tasks;

namespace BlazingChatter.Services
{
    public class DadJokeService : IJokeService
    {
        readonly HttpClient _httpClient;

        public DadJokeService(
            IHttpClientFactory httpClientFactory) =>
            _httpClient = httpClientFactory.CreateClient(nameof(DadJokeService));

        string IJokeService.Actor => "\"Dad\" Joke Bot";

        async ValueTask<string> IJokeService.GetJokeAsync() =>
            await _httpClient.GetStringAsync("https://icanhazdadjoke.com/");
    }
}