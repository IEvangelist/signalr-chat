using System.Threading;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public class CommandSignal : ICommandSignal
    {
        readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public bool IsRecognizedCommand(string message)
        {
            if (message != "joke")
            {
                return false;
            }

            _signal.Release();
            return true;
        }

        public Task WaitCommandAsync(CancellationToken cancellationToken) 
            => _signal.WaitAsync(cancellationToken);
    }
}