using BlazingChatter.Records;

namespace BlazingChatter.Shared
{
    public record ActorCommand(
        string User,
        string OriginalText,
        Command Command) : Actor(User);
}
