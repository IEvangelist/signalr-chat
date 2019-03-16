using System.Threading;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public interface ICommandSignal
    {
        bool IsRecognizedCommand(string message);

        Task WaitCommandAsync(CancellationToken cancellationToken);
    }
}