using IEvangelist.SignalR.Chat.Enums;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Services
{
    public class CommandSignal : ICommandSignal
    {
        AsyncAutoResetEvent _signal = new AsyncAutoResetEvent(false);

        JokeType _activeJokeType = JokeType.Dad;
        BotCommand _activeCommand = BotCommand.None;
        string _lang = "en";

        public bool IsRecognizedCommand(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var commandAndLang = message.Split(":");
            var command = commandAndLang[0];

            _activeJokeType = commandAndLang.Length > 1 ? (JokeType)Enum.Parse(typeof(JokeType), commandAndLang[1], true) : JokeType.Dad;
            _lang = commandAndLang.Length > 2 ? commandAndLang[2] : "en";

            switch (command)
            {
                case "joke":
                    _activeCommand = BotCommand.TellJoke;
                    break;

                case "jokes":
                    _activeCommand = BotCommand.SayJokes;
                    break;

                case "stop":
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

        public void Reset(bool isSet) => 
            _signal = new AsyncAutoResetEvent(isSet);

        public async Task<Command> WaitCommandAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            return (_activeJokeType, _activeCommand, _lang);
        }
    }

    public readonly struct Command
    {
        public readonly JokeType JokeType;
        public readonly BotCommand BotCommand;
        public readonly string Language;

        private Command(
            JokeType type,
            BotCommand command,
            string lang)
        {
            JokeType = type;
            BotCommand = command;
            Language = lang;
        }

        public void Deconstruct(
            out JokeType type, 
            out BotCommand command, 
            out string lang)
        {
            type = JokeType;
            command = BotCommand;
            lang = Language;
        }

        public static implicit operator Command(
            (JokeType type, BotCommand command, string lang) tuple) =>
            new Command(tuple.type, tuple.command, tuple.lang);
    }
}