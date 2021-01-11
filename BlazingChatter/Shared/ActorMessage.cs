namespace BlazingChatter.Shared
{
    public record ActorMessage(
        string Id, string Text, string User,
        string? Lang = null, bool IsGreeting = false, bool IsEdit = false,
        bool IsChatBot = false, bool SayJoke = false) : Actor(User);
}
