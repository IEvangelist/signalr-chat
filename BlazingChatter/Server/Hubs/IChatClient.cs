using BlazingChatter.Shared;
using System.Threading.Tasks;

namespace BlazingChatter.Hubs
{
    public interface IChatClient
    {
        Task UserLoggedOn(Actor actor);

        Task UserLoggedOff(Actor actor);

        Task UserTyping(ActorAction action);

        Task MessageReceived(ActorMessage message);
    }
}
