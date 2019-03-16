using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public interface IDadJokeService
    {
        Task<string> GetDadJokeAsync();
    }
}