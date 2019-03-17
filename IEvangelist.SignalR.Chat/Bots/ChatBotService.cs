using System;
using System.Threading;
using System.Threading.Tasks;
using IEvangelist.SignalR.Chat.Enums;
using IEvangelist.SignalR.Chat.Hubs;
using IEvangelist.SignalR.Chat.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IEvangelist.SignalR.Chat.Bots
{
    public class ChatBotService : BackgroundService
    {
        const string ChatBotUserName = "\"Dad\" Joke Bot";

        readonly IHubContext<ChatHub> _chatHub;
        readonly IDadJokeService _dataJokeService;
        readonly ICommandSignal _commandSignal;
        readonly ILogger<ChatBotService> _logger;
        readonly Random _random = new Random((int)DateTime.Now.Ticks);

        public ChatBotService(
            IHubContext<ChatHub> chatHub,
            IDadJokeService dataJokeService,
            ICommandSignal commandSignal,
            ILogger<ChatBotService> logger)
        {
            _chatHub = chatHub;
            _dataJokeService = dataJokeService;
            _commandSignal = commandSignal;
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
                    var command = await _commandSignal.WaitCommandAsync(cancellationToken);
                    switch (command)
                    {
                        case BotCommand.TellJoke:
                            var tellJoke = await PrepareJokeAsync(cancellationToken);
                            await SendJokeAsync(tellJoke, command, cancellationToken);
                            _commandSignal.Reset(false);
                            break;

                        case BotCommand.SayJokes:
                            var sayJoke = await PrepareJokeAsync(cancellationToken);
                            await SendJokeAsync(sayJoke, command, cancellationToken);
                            await Task.Delay(_random.Next(5000, 30000), cancellationToken);
                            _commandSignal.Reset(true);
                            break;

                        default:
                            await Task.Delay(5000, cancellationToken);
                            continue;
                    }
                }
                catch (Exception ex)
                {
                    // We don't know (or have a way of knowing) if there are actually clients connected.
                    // This happen => "The connection is not active, data cannot be sent to the service."
                    _logger.LogError($"Error: {ex.Message}.", ex);
                }
            }

            await Task.CompletedTask;
        }

        async Task<string> PrepareJokeAsync(CancellationToken cancellationToken)
        {
            await ToggleIsTypingAsync(true, cancellationToken);
            await Task.Delay(2500, cancellationToken);
            var joke = await _dataJokeService.GetDadJokeAsync();
            await ToggleIsTypingAsync(false, cancellationToken);

            return joke;
        }

        Task SendJokeAsync(string joke, BotCommand command, CancellationToken cancellationToken)
            => _chatHub.Clients
                       .All
                       .SendAsync(
                            "MessageReceived",
                            new
                            {
                                text = joke,
                                id = Guid.NewGuid().ToString(),
                                user = ChatBotUserName,
                                isChatBot = true,
                                sayJoke = command == BotCommand.SayJokes
                            },
                            cancellationToken);

        Task ToggleIsTypingAsync(bool isTyping, CancellationToken cancellationToken)
            => _chatHub.Clients
                       .All
                       .SendAsync(
                            "UserTyping",
                            new
                            {
                                isTyping,
                                user = ChatBotUserName
                            },
                            cancellationToken);
    }
}
