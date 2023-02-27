using System.Security.Claims;
using BlazingChatter.Client.Extensions;
using BlazingChatter.Client.Interop;
using BlazingChatter.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using DebounceTimer = System.Timers.Timer;

namespace BlazingChatter.Client.Pages;

public sealed partial class ChatRoom : IAsyncDisposable
{
    readonly Dictionary<string, ActorMessage> _messages = new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<Actor> _usersTyping = new();
    readonly HashSet<IDisposable> _hubRegistrations = new();
    readonly List<double> _voiceSpeeds =
        Enumerable.Range(0, 12).Select(i => (i + 1) * .25).ToList();
    readonly DebounceTimer _debounceTimer = new()
    {
        Interval = 750,
        AutoReset = false
    };

    HubConnection? _hubConnection;

    string? _messageId;
    string? _message;
    ActorMessage? _lastMessage;
    bool _isTyping;

    ActorCommand? _lastCommand;

    ElementReference _messageInput;
    SpeechSynthesisVoice[] _voices = Array.Empty<SpeechSynthesisVoice>();

    public ChatRoom() =>
        _debounceTimer.Elapsed +=
            async (sender, args) => await SetIsTyping(false);

    string Voice
    {
        get => LocalStorage.GetItem<string>("preferred-voice") ?? "Auto";
        set => LocalStorage.SetItem("preferred-voice", value);
    }

    double VoiceSpeed
    {
        get => LocalStorage.GetItem<double?>("preferred-speed") ?? 1.5;
        set => LocalStorage.SetItem("preferred-speed", value);
    }

    [Parameter, EditorRequired]
    public required ClaimsPrincipal User { get; set; }

    [Inject]
    public required NavigationManager Nav { get; set; }

    [Inject]
    public required IJSRuntime JavaScript { get; set; }

    [Inject]
    public required ISpeechSynthesisService SpeechSynthesis { get; set; }

    [Inject]
    public required ILocalStorageService LocalStorage { get; set; }

    [Inject]
    public required ILogger<ChatRoom> Log { get; set; }

    [Inject]
    public required IAccessTokenProvider TokenProvider { get; set; }

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

        await GetVoicesAsync();
        SpeechSynthesis.OnVoicesChanged(() => GetVoicesAsync(true));
    }

    void OnCommandSignalReceived(ActorCommand command) =>
        (_lastCommand, _lastMessage) = (command, null);

    async ValueTask<string?> GetAccessTokenValueAsync()
    {
        var result = await TokenProvider.RequestAccessToken();
        return result.TryGetToken(out var accessToken) ? accessToken.Value : null;
    }

    async Task OnMessageReceivedAsync(ActorMessage message) =>
        await InvokeAsync(
            async () =>
            {
                if (OwnsMessage(message.User))
                {
                    _lastMessage = message;
                    _lastCommand = null;
                }

                _messages[message.Id] = message;
                if (message.IsChatBot && message.SayJoke)
                {
                    var lang = message.Lang ?? "en";
                    var voice = _voices?.FirstOrDefault(v => v.Name == Voice);
                    if (voice is not null)
                    {
                        if (!voice.Lang.StartsWith(lang) && _voices is { Length: > 0 })
                        {
                            var firstLocaleMatchingVoice = _voices.FirstOrDefault(v => v.Lang.StartsWith(lang));
                            if (firstLocaleMatchingVoice is not null)
                            {
                                lang = firstLocaleMatchingVoice.Lang[0..2];
                            }
                        }
                    }

                    SpeechSynthesis.Speak(new SpeechSynthesisUtterance
                    {
                        Text = message.Text,
                        Voice = voice,
                        Rate = VoiceSpeed,
                        Lang = lang
                    });
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

        if (args is { Key: "ArrowUp" })
        {
            if (_lastMessage is not null)
            {
                await StartEdit(_lastMessage);
            }
            else if (_lastCommand is not null)
            {
                _message = _lastCommand.OriginalText;
            }
        }
    }

    async Task SendMessage()
    {
        if (_message is { Length: > 0 })
        {
            await (_hubConnection?.InvokeAsync("PostMessage", _message, _messageId)
                ?? Task.CompletedTask);

            _message = null;
            _messageId = null;

            StateHasChanged();
        }
    }

    async Task InitiateDebounceUserIsTyping()
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();

        await SetIsTyping(true);
    }

    Task SetIsTyping(bool isTyping)
    {
        if (_isTyping && isTyping)
        {
            return Task.CompletedTask;
        }

        Log.LogInformation("Setting is typing: {IsTyping}", isTyping);

        return _hubConnection?.InvokeAsync("UserTyping", _isTyping = isTyping)
            ?? Task.CompletedTask;
    }

    async Task AppendToMessage(string text)
    {
        _message += text;

        await _messageInput.FocusAsync();
        await SetIsTyping(false);
    }

    bool OwnsMessage(string user) => User?.Identity?.Name == user;

    async Task StartEdit(ActorMessage message)
    {
        if (OwnsMessage(message.User) is false)
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

    async Task GetVoicesAsync(bool isFromCallback = false)
    {
        _voices = await SpeechSynthesis.GetVoicesAsync();
        if (_voices is { } && isFromCallback)
        {
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_debounceTimer is { })
        {
            _debounceTimer.Stop();
            _debounceTimer.Dispose();
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
