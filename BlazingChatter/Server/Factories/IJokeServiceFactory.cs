using BlazingChatter.Enums;
using BlazingChatter.Services;

namespace BlazingChatter.Factories
{
    public interface IJokeServiceFactory
    {
        IJokeService Get(JokeType type);
    }
}