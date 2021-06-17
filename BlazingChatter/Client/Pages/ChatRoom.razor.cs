using BlazingChatter.Client.Extensions;
using BlazingChatter.Client.Interop;
using BlazingChatter.Extensions;
using BlazingChatter.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Timers;

namespace BlazingChatter.Client.Pages
{
    public partial class ChatRoom : IAsyncDisposable
    {
        readonly Dictionary<string, ActorMessage> _messages = new(StringComparer.OrdinalIgnoreCase);
        readonly HashSet<Actor> _usersTyping = new();
        readonly HashSet<IDisposable> _hubRegistrations = new();
        readonly List<double> _voiceSpeeds =
            Enumerable.Range(0, 12).Select(i => (i + 1) * .25).ToList();
        readonly Timer _debouceTimer = new()
        {
            Interval = 750,
            AutoReset = false
        };

        HubConnection _hubConnection;

        string _messageId;
        string _message;
        bool _isTyping;

        ActorCommand _lastCommand;

        ElementReference _messageInput;
        List<SpeechSynthesisVoice> _voices;
        string _voice = "Auto";
        double _voiceSpeed = 1;

        public ChatRoom() =>
            _debouceTimer.Elapsed +=
                async (sender, args) => await SetIsTyping(false);

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

        [Inject]
        public IAccessTokenProvider TokenProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Nav.ToAbsoluteUri("/chat"),
                    options => options.AccessTokenProvider =
                        async () => await GetAccessTokenValueAsync())
                .WithAutomaticReconnect()
                .AddMessagePackProtocol()
                .Build();

            _hubRegistrations.Add(_hubConnection.OnMessageReceived(OnMessageReceivedAsync));
            _hubRegistrations.Add(_hubConnection.OnUserTyping(OnUserTypingAsync));
            _hubRegistrations.Add(
                _hubConnection.OnCommandSignalReceived(OnCommandSignalReceived));
            _hubRegistrations.Add(_hubConnection.OnUserLoggedOn(
                actor => JavaScript.NotifyAsync("Hey!", $"{actor.User} logged on...")));
            _hubRegistrations.Add(_hubConnection.OnUserLoggedOff(
                actor => JavaScript.NotifyAsync("Bye!", $"{actor.User} logged off...")));

            await _hubConnection.StartAsync();
            await _messageInput.FocusAsync();

            await UpdateClientVoices(
                await JavaScript.GetClientVoices(this));
        }

        void OnCommandSignalReceived(ActorCommand command) => _lastCommand = command;

        async ValueTask<string> GetAccessTokenValueAsync()
        {
            var result = await TokenProvider.RequestAccessToken();
            return result.TryGetToken(out var accessToken) ? accessToken.Value : null;
        }

        async Task OnMessageReceivedAsync(ActorMessage message) =>
            await InvokeAsync(
                async () =>
                {
                    _messages[message.Id] = message;
                    if (message.IsChatBot && message.SayJoke)
                    {
                        var lang = message.Lang;
                        var voice = _voices?.FirstOrDefault(v => v.Name == _voice);
                        if (voice is not null)
                        {
                            if (!voice.Lang.StartsWith(lang))
                            {
                                var firstLocaleMatchingVoice = _voices.FirstOrDefault(v => v.Lang.StartsWith(lang));
                                if (firstLocaleMatchingVoice is not null)
                                {
                                    lang = firstLocaleMatchingVoice.Lang[0..2];
                                }
                            }
                        }

                        await JavaScript.SpeakAsync(message.Text, _voice, _voiceSpeed, lang);
                    }

                    await JavaScript.ScrollIntoViewAsync();

                    StateHasChanged();
                });

        async Task OnUserTypingAsync(ActorAction actorAction) =>
            await InvokeAsync(() =>
            {
                var (user, isTyping) = actorAction;
                _ = isTyping
                    ? _usersTyping.Add(new(user))
                    : _usersTyping.Remove(new(user));

                StateHasChanged();
            });

        async Task OnKeyUp(KeyboardEventArgs args)
        {
            if (args is { Key: "Enter" } and { Code: "Enter" })
            {
                await SendMessage();
            }

            if (args is { Key: "ArrowUp" } && _lastCommand is not null)
            {
                _message = _lastCommand.OriginalText;
            }
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

        async Task InitiateDebounceUserIsTyping()
        {
            _debouceTimer.Stop();
            _debouceTimer.Start();

            await SetIsTyping(true);
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

            await _messageInput.FocusAsync();
            await SetIsTyping(false);
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

                    await _messageInput.FocusAsync();

                    StateHasChanged();
                });
        }

        [JSInvokable]
        public async Task UpdateClientVoices(string voicesJson) =>
            await InvokeAsync(() =>
            {
                var voices = voicesJson.FromJson<List<SpeechSynthesisVoice>>();
                if (voices is { Count: > 0 })
                {
                    _voices = voices;

                    StateHasChanged();
                }
            });

        public async ValueTask DisposeAsync()
        {
            if (_debouceTimer is { })
            {
                _debouceTimer.Stop();
                _debouceTimer.Dispose();
            }

            if (_hubRegistrations is { Count: > 0 })
            {
                foreach (var disposable in _hubRegistrations)
                {
                    disposable.Dispose();
                }
            }

            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
