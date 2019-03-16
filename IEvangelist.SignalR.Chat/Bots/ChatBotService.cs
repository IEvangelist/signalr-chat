using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using IEvangelist.SignalR.Chat.Hubs;
using IEvangelist.SignalR.Chat.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IEvangelist.SignalR.Chat.Bots
{
    public class ChatBotService : BackgroundService
    {
        const string ChatBotUserName = "Joke Bot";

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
                    await _commandSignal.WaitCommandAsync(cancellationToken);

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    await ToggleIsTypingAsync(true, cancellationToken);
                    await Task.Delay(2500, cancellationToken);

                    _logger.LogInformation($"Joke bot was typing for {stopwatch.Elapsed}");
                    stopwatch.Restart();
                    
                    var joke = await _dataJokeService.GetDadJokeAsync();
                    _logger.LogInformation($"Got a joke, took {stopwatch.Elapsed} to think of one...{joke}.");
                    stopwatch.Restart();

                    await ToggleIsTypingAsync(false, cancellationToken);
                    await _chatHub.Clients
                                  .All
                                  .SendAsync(
                                       "MessageReceived",
                                       new
                                       {
                                           text = joke,
                                           id = Guid.NewGuid().ToString(),
                                           user = ChatBotUserName,
                                           isChatBot = true
                                       },
                                       cancellationToken);

                    stopwatch.Stop();
                    _logger.LogInformation($"Joke bot, shared his joke after {stopwatch.Elapsed}");
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
