namespace BlazingChatter.Shared
{
    public record ActorAction(string User, bool IsTyping) : Actor(User);
}
