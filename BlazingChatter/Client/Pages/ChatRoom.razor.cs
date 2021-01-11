using BlazingChatter.Client.Interop;
using BlazingChatter.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazingChatter.Client.Pages
{
    public partial class ChatRoom
    {
        readonly Dictionary<string, ActorMessage> _messages = new(StringComparer.OrdinalIgnoreCase);
        readonly HashSet<Actor> _usersTyping = new();
        readonly string _inputElementId = "message-input";
        readonly List<double> _voiceSpeeds =
            Enumerable.Range(0, 12).Select(i => (i + 1) * .25).ToList();

        HubConnection _hubConnection;
        string _messageId;
        string _message;
        bool _isTyping;

        List<SpeechSynthesisVoice> _voices;
        string _voice = "Auto";
        double _voiceSpeed = 1;

        [Parameter]
        public ClaimsPrincipal User { get; set; }

        [Inject]
        public NavigationManager Nav { get; set; }

        [Inject]
        public IJSRuntime JavaScript { get; set; }

        [Inject]
        public HttpClient Http { get; set; }

        [Inject]
        public ILogger<ChatRoom> Log { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(
                    Nav.ToAbsoluteUri("/chat"),
                    options =>
                        options.AccessTokenProvider =
                            async () =>
                            {
                                var token = await Http.GetStringAsync("genaratetoken");
                                Log.LogInformation($"Server token: {token}");

                                return token;
                            })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<ActorMessage>("MessageReceived", OnMessageReceivedAsync);
            _hubConnection.On<Actor>("UserLoggedOn",
                async actor => await JavaScript.NotifyAsync("Hey!", $"{actor.User} logged on..."));
            _hubConnection.On<Actor>("UserLoggedOff",
                async actor => await JavaScript.NotifyAsync("Bye!", $"{actor.User} logged off..."));
            _hubConnection.On<ActorAction>("UserTyping", OnUserTypingAsync);

            await _hubConnection.StartAsync();
            await JavaScript.FocusAsync(_inputElementId);
            await UpdateClientVoices(
                await JavaScript.GetClientVoices(this));
        }

        async Task SendMessage()
        {
            if (_message is { Length: > 0 })
            {
                await _hubConnection.InvokeAsync("PostMessage", _message, _messageId);

                _message = null;
                _messageId = null;

                StateHasChanged();
            }
        }

        async Task SetIsTyping(bool isTyping)
        {
            if (_isTyping && isTyping)
            {
                return;
            }

            Log.LogInformation($"Setting is typing: {isTyping}");

            await _hubConnection.InvokeAsync("UserTyping", _isTyping = isTyping);
        }

        async Task AppendToMessage(string text)
        {
            _message += text;

            await JavaScript.FocusAsync(_inputElementId);
            await SetIsTyping(false);
        }

        async Task OnMessageReceivedAsync(ActorMessage message)
        {
            await InvokeAsync(
                async () =>
                {
                    _messages[message.Id] = message;

                    if (message.IsChatBot && message.SayJoke)
                    {
                        await JavaScript.SpeakAsync(message.Text, _voice, _voiceSpeed, message.Lang);
                    }

                    await JavaScript.ScrollIntoViewAsync();

                    StateHasChanged();
                });
        }

        async Task OnUserTypingAsync(ActorAction actorAction)
        {
            await InvokeAsync(() =>
            {
                var (user, isTyping) = actorAction;

                Log.LogInformation($"User: {user} is typing value: {isTyping}");

                if (isTyping)
                {
                    _usersTyping.Add(new(user));
                }
                else
                {
                    _usersTyping.Remove(new(user));
                }

                StateHasChanged();
            });
        }

        bool OwnsMessage(string user) => User.Identity.Name == user;

        async Task StartEdit(ActorMessage message)
        {
            if (!OwnsMessage(message.User))
            {
                return;
            }

            await InvokeAsync(
                async () =>
                {
                    _messageId = message.Id;
                    _message = message.Text;

                    await JavaScript.FocusAsync(_inputElementId);

                    StateHasChanged();
                });
        }

        [JSInvokable]
        public async Task UpdateClientVoices(
            List<SpeechSynthesisVoice> voices) =>
            await InvokeAsync(() =>
            {
                _voices = voices;

                StateHasChanged();
            });
    }
}
