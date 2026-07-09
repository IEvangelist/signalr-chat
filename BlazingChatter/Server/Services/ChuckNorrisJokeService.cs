using BlazingChatter.Extensions;
using BlazingChatter.Records;

namespace BlazingChatter.Services;

public class ChuckNorrisJokeService : IJokeService
{
    readonly HttpClient _httpClient;

    public ChuckNorrisJokeService(
        IHttpClientFactory httpClientFactory) =>
        _httpClient = httpClientFactory.CreateClient(nameof(ChuckNorrisJokeService));

    string IJokeService.Actor => "\"Chuck Norris\" Joke Bot";

    async ValueTask<string> IJokeService.GetJokeAsync()
    {
        try
        {
            var content = await _httpClient.GetStringAsync(
                "https://api.chucknorris.io/jokes/random?category=dev");
            var result = content.FromJson<ChuckNorrisJoke>();

            return result?.Value is { Length: > 0 } joke
                ? joke
                : "Chuck Norris is speechless right now — try again!";
        }
        catch (Exception)
        {
            return "Chuck Norris roundhouse-kicked the joke server. Try again!";
        }
    }
}