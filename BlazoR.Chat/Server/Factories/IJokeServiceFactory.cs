using BlazorR.Chat.Enums;
using BlazorR.Chat.Services;

namespace BlazorR.Chat.Factories
{
    public interface IJokeServiceFactory
    {
        IJokeService Get(JokeType type);
    }
}