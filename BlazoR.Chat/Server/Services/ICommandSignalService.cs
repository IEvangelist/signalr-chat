using System.Threading;
using System.Threading.Tasks;
using BlazorR.Chat.Records;

namespace BlazorR.Chat.Services
{
    public interface ICommandSignalService
    {
        bool IsRecognizedCommand(string message);

        void Reset(bool isSet);

        Task<Command> WaitCommandAsync(CancellationToken cancellationToken);
    }
}