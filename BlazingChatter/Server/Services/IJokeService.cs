using System.Threading.Tasks;

namespace BlazingChatter.Services
{
    public interface IJokeService
    {
        string Actor { get; }

        ValueTask<string> GetJokeAsync();
    }
}