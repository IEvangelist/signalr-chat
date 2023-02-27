namespace BlazingChatter.Services;

internal sealed class DadJokeService : IJokeService
{
    readonly HttpClient _httpClient;

    public DadJokeService(
        IHttpClientFactory httpClientFactory) =>
        _httpClient = httpClientFactory.CreateClient(nameof(DadJokeService));

    string IJokeService.Actor => "\"Dad\" Joke Bot";

    async ValueTask<string> IJokeService.GetJokeAsync() =>
        await _httpClient.GetStringAsync("https://icanhazdadjoke.com/");
}