using System.Net.Http;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public class DadJokeService : IDadJokeService
    {
        readonly HttpClient _httpClient;

        public DadJokeService(HttpClient httpClient) => _httpClient = httpClient;

        public Task<string> GetDadJokeAsync()
            => _httpClient.GetStringAsync("https://icanhazdadjoke.com/");
    }
}