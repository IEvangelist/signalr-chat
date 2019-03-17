using IEvangelist.SignalR.Chat.Enums;
using Nito.AsyncEx;
using System.Threading;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public class CommandSignal : ICommandSignal
    {
        AsyncAutoResetEvent _signal = new AsyncAutoResetEvent(false);
        BotCommand _activeCommand = BotCommand.None;

        public bool IsRecognizedCommand(string message)
        {
            switch (message)
            {
                case "joke":
                case "tell:joke":
                    _activeCommand = BotCommand.TellJoke;
                    break;

                case "jokes":
                case "say:jokes":
                    _activeCommand = BotCommand.SayJokes;
                    break;

                case "stop":
                case "stop:jokes":
                    _activeCommand = BotCommand.None;
                    break;

                default:
                    return false;
            }

            if (_activeCommand != BotCommand.None)
            {
                _signal.Set();
            }

            return true;
        }

        public void Reset(bool isSet) => _signal = new AsyncAutoResetEvent(isSet);

        public async Task<BotCommand> WaitCommandAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            return _activeCommand;
        }
    }
}