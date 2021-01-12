using System.Threading;
using System.Threading.Tasks;
using BlazingChatter.Records;

namespace BlazingChatter.Services
{
    public interface ICommandSignalService
    {
        bool IsRecognizedCommand(string message);

        void Reset(bool isSet);

        Task<Command> WaitCommandAsync(CancellationToken cancellationToken);
    }
}