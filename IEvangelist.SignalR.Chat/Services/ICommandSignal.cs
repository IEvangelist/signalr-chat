using System.Threading;
using System.Threading.Tasks;
using IEvangelist.SignalR.Chat.Enums;

namespace IEvangelist.SignalR.Chat.Services
{
    public interface ICommandSignal
    {
        bool IsRecognizedCommand(string message);

        void Reset(bool isSet);

        Task<Command> WaitCommandAsync(CancellationToken cancellationToken);
    }
}