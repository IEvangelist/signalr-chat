using IEvangelist.SignalR.Chat.Enums;
using IEvangelist.SignalR.Chat.Services;

namespace IEvangelist.SignalR.Chat.Providers
{
    public interface IJokeServiceProvider
    {
        IJokeService Get(JokeType type);
    }
}