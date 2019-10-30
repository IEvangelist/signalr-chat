using System;
using System.Threading;
using System.Threading.Tasks;
using IEvangelist.SignalR.Chat.Enums;
using IEvangelist.SignalR.Chat.Hubs;
using IEvangelist.SignalR.Chat.Providers;
using IEvangelist.SignalR.Chat.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IEvangelist.SignalR.Chat.Bots
{
    public class ChatBotService : BackgroundService
    {
        readonly IHubContext<ChatHub> _chatHub;
        readonly IJokeServiceProvider _jokeServiceProvider;
        readonly ICommandSignal _commandSignal;
        readonly ITranslationService _translationService;
        readonly ILogger<ChatBotService> _logger;
        readonly Random _random = new Random((int)DateTime.Now.Ticks);

        public ChatBotService(
            IHubContext<ChatHub> chatHub,
            IJokeServiceProvider jokeServiceProvider,
            ICommandSignal commandSignal,
            ITranslationService translationService,
            ILogger<ChatBotService> logger)
        {
            _chatHub = chatHub;
            _jokeServiceProvider = jokeServiceProvider;
            _commandSignal = commandSignal;
            _translationService = translationService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation("Joke Bot, awaiting command signal...");

                try
                {
                    var (type, command, lang) = await _commandSignal.WaitCommandAsync(cancellationToken);
                    switch (command)
                    {
                        case BotCommand.TellJoke:
                            {
                                var (joke, bot) = await PrepareJokeAsync(type, lang, cancellationToken);
                                await SendJokeAsync(joke, bot, lang, cancellationToken);
                                _commandSignal.Reset(false);
                            }
                            break;

                        case BotCommand.SayJokes:
                            {
                                var (joke, bot) = await PrepareJokeAsync(type, lang, cancellationToken);
                                await SendJokeAsync(joke, bot, lang, cancellationToken);
                                await Task.Delay(_random.Next(7500, 15000), cancellationToken);
                                _commandSignal.Reset(true);
                            }
                            break;

                        default:
                            await Task.Delay(5000, cancellationToken);
                            continue;
                    }
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

        async Task<(string joke, string user)> PrepareJokeAsync(JokeType type, string lang, CancellationToken cancellationToken)
        {
            var svc = _jokeServiceProvider.Get(type);
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
            _chatHub.Clients
                    .All
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
            _chatHub.Clients
                    .All
                    .SendAsync(
                         "UserTyping",
                         new
                         {
                             isTyping,
                             user = bot
                         },
                         cancellationToken);
    }
}
