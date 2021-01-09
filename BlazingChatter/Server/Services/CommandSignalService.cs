using Nito.AsyncEx;
using BlazingChatter.Enums;
using BlazingChatter.Records;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazingChatter.Services
{
    public class CommandSignalService : ICommandSignalService
    {
        AsyncAutoResetEvent _signal = new(false);
        JokeType _activeJokeType = JokeType.Dad;
        BotCommand _activeCommand = BotCommand.None;
        string _lang = "en";

        bool ICommandSignalService.IsRecognizedCommand(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var isRecognized = true;
            var commandAndLang = message.Split(":");
            var command = commandAndLang[0];

            _activeJokeType = commandAndLang.Length > 1 ? (JokeType)Enum.Parse(typeof(JokeType), commandAndLang[1], true) : JokeType.Dad;
            _lang = commandAndLang.Length > 2 ? commandAndLang[2] : "en";
            _activeCommand = command switch
            {
                "joke" => BotCommand.TellJoke,
                "jokes" => BotCommand.SayJokes,
                "stop" => BotCommand.None,
                var _ when (isRecognized = false) == false => _activeCommand,
                _ => _activeCommand
            };

            if (_activeCommand != BotCommand.None)
            {
                _signal.Set();
            }

            return isRecognized;
        }

        void ICommandSignalService.Reset(bool isSet) => _signal = new(isSet);

        async Task<Command> ICommandSignalService.WaitCommandAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            return (_activeJokeType, _activeCommand, _lang);
        }
    }
}