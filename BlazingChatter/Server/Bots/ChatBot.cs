using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BlazingChatter.Enums;
using BlazingChatter.Factories;
using BlazingChatter.Hubs;
using BlazingChatter.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using BlazingChatter.Shared;

namespace BlazingChatter.Bots
{
    public class ChatBot : BackgroundService
    {
        readonly IHubContext<ChatHub, IChatClient> _chatHub;
        readonly IJokeServiceFactory _jokeServiceFactory;
        readonly ICommandSignalService _commandSignalService;
        readonly ITranslationService _translationService;
        readonly ILogger<ChatBot> _logger;
        readonly Random _random = new((int)DateTime.Now.Ticks);

        public ChatBot(
            IHubContext<ChatHub, IChatClient> chatHub,
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
                    await SendJokeAsync(joke, bot, language);
                    _commandSignalService.Reset(false);
                }

                async Task SayJokesAsync(
                    JokeType jokeType, string language, CancellationToken token)
                {
                    var (joke, bot) = await PrepareJokeAsync(jokeType, language, token);
                    await SendJokeAsync(joke, bot, language);
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

            await ToggleIsTypingAsync(true, bot);
            await Task.Delay(_random.Next(1000, 3000), cancellationToken);

            var joke = await svc.GetJokeAsync();
            await ToggleIsTypingAsync(false, bot);

            if (lang is { Length: > 0 } && lang != "en-US")
            {
                var (translatedJoke, _) = await _translationService.TranslateAsync(joke, lang);
                return (translatedJoke, bot);
            }

            return (joke, bot);
        }

        Task SendJokeAsync(string joke, string bot, string lang) =>
            _chatHub.Clients.All
                .MessageReceived(
                new ActorMessage(
                    Id: Guid.NewGuid().ToString(),
                    Text: joke,
                    User: bot,
                    Lang: lang,
                    IsChatBot: true,
                    SayJoke: true));

        Task ToggleIsTypingAsync(bool isTyping, string bot) =>
            _chatHub.Clients.All.UserTyping(new ActorAction(bot, isTyping));
    }
}
