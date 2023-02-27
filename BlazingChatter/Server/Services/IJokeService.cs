namespace BlazingChatter.Services;

public interface IJokeService
{
    string Actor { get; }

    ValueTask<string> GetJokeAsync();
}