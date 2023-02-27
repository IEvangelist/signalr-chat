using Nito.AsyncEx;
using BlazingChatter.Enums;
using BlazingChatter.Records;
using BlazingChatter.Shared;

namespace BlazingChatter.Services;

internal sealed class CommandSignalService : ICommandSignalService
{
    AsyncAutoResetEvent _signal = new(false);
    JokeType _activeJokeType = JokeType.Dad;
    BotCommand _activeCommand = BotCommand.None;
    string _lang = "en";

    bool ICommandSignalService.IsRecognizedCommand(
        string user, string message, out ActorCommand? actorCommand)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            actorCommand = null;
            return false;
        }

        var isRecognized = true;
        var commandAndLang = message.Split(":");
        var command = commandAndLang[0];

        static JokeType ParseJokeType(string value) => value switch
        { 
            "dad" or "d" => JokeType.Dad,
            "chucknorris" or "cn" => JokeType.ChuckNorris,

            _ => JokeType.Dad
        };
        _activeJokeType = commandAndLang.Length > 1 ? ParseJokeType(commandAndLang[1]) : JokeType.Dad;
        _lang = commandAndLang.Length > 2 ? commandAndLang[2] : "en";
        _activeCommand = command switch
        {
            "joke" => BotCommand.TellJoke,
            "jokes" => BotCommand.SayJokes,
            "stop" => BotCommand.None,
            var _ when (isRecognized = false) == false => BotCommand.None,
            _ => BotCommand.None
        };

        if (_activeCommand != BotCommand.None)
        {
            _signal.Set();
        }

        actorCommand = new ActorCommand(
            user, message, Command: (_activeJokeType, _activeCommand, _lang));

        return isRecognized;
    }

    void ICommandSignalService.Reset(bool isSet) => _signal = new(isSet);

    async ValueTask<Command> ICommandSignalService.WaitCommandAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        return (_activeJokeType, _activeCommand, _lang);
    }
}