using System.Threading.Tasks;

namespace BlazorR.Chat.Services
{
    public interface IJokeService
    {
        string Actor { get; }

        ValueTask<string> GetJokeAsync();
    }
}