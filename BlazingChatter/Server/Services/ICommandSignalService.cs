using BlazingChatter.Records;
using BlazingChatter.Shared;

namespace BlazingChatter.Services;

public interface ICommandSignalService
{
    bool IsRecognizedCommand(string user, string message, out ActorCommand? actorCommand);

    void Reset(bool isSet);

    ValueTask<Command> WaitCommandAsync(CancellationToken cancellationToken);
}