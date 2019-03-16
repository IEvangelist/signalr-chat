using System;
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
        readonly ILogger<ChatBotService> _logger;
        readonly Random _random = new Random((int)DateTime.Now.Ticks);

        public ChatBotService(
            IHubContext<ChatHub> chatHub,
            IDadJokeService dataJokeService,
            ILogger<ChatBotService> logger)
        {
            _chatHub = chatHub;
            _dataJokeService = dataJokeService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ToggleIsTypingAsync(true, cancellationToken);
                    await Task.Delay(3500, cancellationToken);

                    var joke = await _dataJokeService.GetDadJokeAsync();
                    _logger.LogInformation($"Joke: {joke}.");

                    await ToggleIsTypingAsync(false, cancellationToken);
                    await _chatHub.Clients
                                  .All
                                  .SendAsync(
                                       "MessageReceived",
                                       new
                                       {
                                           text = joke,
                                           id = Guid.NewGuid().ToString(),
                                           user = ChatBotUserName
                                       },
                                       cancellationToken);
                }
                catch (Exception ex)
                {
                    // We don't know (or have a way of knowing) if there are actually clients connected.
                    // This happen => "The connection is not active, data cannot be sent to the service."
                    _logger.LogError($"Error: {ex.Message}.", ex);
                }
                finally
                {
                    await Task.Delay(_random.Next(3000, 60000), cancellationToken);
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
