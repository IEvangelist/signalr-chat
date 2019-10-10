using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public interface IJokeService
    {
        string Actor { get; }

        Task<string> GetJokeAsync();
    }
}