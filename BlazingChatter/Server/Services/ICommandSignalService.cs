using System.Threading;
using System.Threading.Tasks;
using BlazingChatter.Records;
using BlazingChatter.Shared;

namespace BlazingChatter.Services
{
    public interface ICommandSignalService
    {
        bool IsRecognizedCommand(string user, string message, out ActorCommand? actorCommand);

        void Reset(bool isSet);

        Task<Command> WaitCommandAsync(CancellationToken cancellationToken);
    }
}