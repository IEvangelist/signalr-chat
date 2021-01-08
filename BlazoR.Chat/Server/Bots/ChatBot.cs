using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BlazorR.Chat.Enums;
using BlazorR.Chat.Factories;
using BlazorR.Chat.Hubs;
using BlazorR.Chat.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorR.Chat.Bots
{
    public class ChatBot : BackgroundService
    {
        readonly IHubContext<ChatHub> _chatHub;
        readonly IJokeServiceFactory _jokeServiceFactory;
        readonly ICommandSignalService _commandSignalService;
        readonly ITranslationService _translationService;
        readonly ILogger<ChatBot> _logger;
        readonly Random _random = new((int)DateTime.Now.Ticks);

        public ChatBot(
            IHubContext<ChatHub> chatHub,
            IJokeServiceFactory jokeServiceProvider,
            ICommandSignalService commandSignal,
            ITranslationService translationService,
            ILogger<ChatBot> logger)
        {
            _chatHub = chatHub;
            _jokeServiceFactory = jokeServiceProvider;
            _commandSignalService = commandSignal;
            _translationService = translationService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation("Joke Bot, awaiting command signal...");

                async Task TellJokeAsync(
                    JokeType jokeType, string language, CancellationToken token)
                {
                    var (joke, bot) = await PrepareJokeAsync(jokeType, language, token);
                    await SendJokeAsync(joke, bot, language, token);
                    _commandSignalService.Reset(false);
                }

                async Task SayJokesAsync(
                    JokeType jokeType, string language, CancellationToken token)
                {
                    var (joke, bot) = await PrepareJokeAsync(jokeType, language, token);
                    await SendJokeAsync(joke, bot, language, cancellationToken);
                    await Task.Delay(_random.Next(7500, 15000), cancellationToken);
                    _commandSignalService.Reset(true);
                }

                try
                {
                    var (type, command, lang) = await _commandSignalService.WaitCommandAsync(cancellationToken);
                    var task = command switch
                    {
                        BotCommand.TellJoke => TellJokeAsync(type, lang, cancellationToken),
                        BotCommand.SayJokes => SayJokesAsync(type, lang, cancellationToken),
                        _ => Task.Delay(5000, cancellationToken)
                    };

                    await task;
                }
                catch (Exception ex)
                {
                    // We don't know (or have a way of knowing) if there are actually clients connected.
                    // This happens => "The connection is not active, data cannot be sent to the service."
                    _logger.LogError($"Error: {ex.Message}.", ex);
                }
            }

            await Task.CompletedTask;
        }

        async Task<(string joke, string user)> PrepareJokeAsync(
            JokeType type, string lang, CancellationToken cancellationToken)
        {
            var svc = _jokeServiceFactory.Get(type);
            var bot = svc.Actor;

            await ToggleIsTypingAsync(true, bot, cancellationToken);
            await Task.Delay(_random.Next(1000, 3000), cancellationToken);

            var joke = await svc.GetJokeAsync();
            await ToggleIsTypingAsync(false, bot, cancellationToken);

            if (lang != "en-US")
            {
                var (translatedJoke, _) = await _translationService.TranslateAsync(joke, lang);
                return (translatedJoke, bot);
            }

            return (joke, bot);
        }

        Task SendJokeAsync(string joke, string bot, string lang, CancellationToken cancellationToken) =>
            _chatHub.Clients.All
                .SendAsync(
                "MessageReceived",
                new
                {
                    text = joke,
                    lang,
                    id = Guid.NewGuid().ToString(),
                    user = bot,
                    isChatBot = true,
                    sayJoke = true
                },
                cancellationToken);

        Task ToggleIsTypingAsync(bool isTyping, string bot, CancellationToken cancellationToken) =>
            _chatHub.Clients.All
                .SendAsync(
                "UserTyping",
                new { isTyping, user = bot },
                cancellationToken);
    }
}
