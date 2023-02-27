using BlazingChatter.Shared;

namespace BlazingChatter.Hubs;

public interface IChatClient
{
    Task UserLoggedOn(Actor actor);

    Task UserLoggedOff(Actor actor);

    Task UserTyping(ActorAction action);

    Task MessageReceived(ActorMessage message);

    Task CommandSignalReceived(ActorCommand command);
}
