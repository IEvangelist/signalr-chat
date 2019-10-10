using IEvangelist.SignalR.Chat.Extensions;
using System.Net.Http;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public class ChuckNorrisJokeService : IJokeService
    {
        readonly HttpClient _httpClient;

        public ChuckNorrisJokeService(IHttpClientFactory httpClientFactory) => 
            _httpClient = httpClientFactory.CreateClient(nameof(ChuckNorrisJokeService));

        string IJokeService.Actor => "\"Chuck Norris\" Joke Bot";

        async Task<string> IJokeService.GetJokeAsync()
        {
            var content = await _httpClient.GetStringAsync("http://api.icndb.com/jokes/random?limitTo=[nerdy]");
            var result = content.FromJson<JokeApiResult>();

            return result?.Value?.Joke ?? "Oops, that didn't work";
        }
    }

    public class JokeApiResult
    {
        public string Type { get; set; }
        public Value Value { get; set; }
    }

    public class Value
    {
        public int Id { get; set; }

        private string _joke;

        public string Joke
        {
            get => _joke;
            set => _joke = value?.Replace("&quot;", "\"");
        }
    }
}