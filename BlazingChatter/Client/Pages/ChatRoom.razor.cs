using BlazingChatter.Client.Extensions;
using BlazingChatter.Client.Interop;
using BlazingChatter.Extensions;
using BlazingChatter.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

        ElementReference _messageInput;
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
                .WithUrl(Nav.ToAbsoluteUri("/chat"))
                .WithAutomaticReconnect()
                .AddMessagePackProtocol()
                .Build();

            _hubConnection.OnMessageReceived(OnMessageReceivedAsync);
            _hubConnection.OnUserTyping(OnUserTypingAsync);

            _hubConnection.OnUserLoggedOn(
                async actor => await JavaScript.NotifyAsync("Hey!", $"{actor.User} logged on..."));
            _hubConnection.OnUserLoggedOff(
                async actor => await JavaScript.NotifyAsync("Bye!", $"{actor.User} logged off..."));

            await _hubConnection.StartAsync();
            await _messageInput.FocusAsync();

            await UpdateClientVoices(
                await JavaScript.GetClientVoices(this));
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

        async Task OnKeyUp(KeyboardEventArgs args)
        {
            if (args is { Key: "Enter" } and { Code: "Enter" })
            {
                await SendMessage();
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

            await JavaScript.FocusElementAsync(_inputElementId);
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

                    await JavaScript.FocusElementAsync(_inputElementId);

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
    }
}
