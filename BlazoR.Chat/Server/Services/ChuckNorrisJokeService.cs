using BlazorR.Chat.Extensions;
using BlazorR.Chat.Records;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorR.Chat.Services
{
    public class ChuckNorrisJokeService : IJokeService
    {
        readonly HttpClient _httpClient;

        public ChuckNorrisJokeService(
            IHttpClientFactory httpClientFactory) =>
            _httpClient = httpClientFactory.CreateClient(nameof(ChuckNorrisJokeService));

        string IJokeService.Actor => "\"Chuck Norris\" Joke Bot";

        async ValueTask<string> IJokeService.GetJokeAsync()
        {
            var content = await _httpClient.GetStringAsync("http://api.icndb.com/jokes/random?limitTo=[nerdy]");
            var result = content.FromJson<JokeApiResponse>();

            return result?.Value?.Joke ?? "Oops, that didn't work";
        }
    }
}