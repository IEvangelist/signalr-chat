using Microsoft.AspNetCore.SignalR;
using BlazingChatter.Services;
using BlazingChatter.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;

namespace BlazingChatter.Hubs;

[Authorize, RequiredScope(new[] { "user_chat" })]
public sealed class ChatHub : Hub<IChatClient>
{
    readonly ICommandSignalService _commandSignal;

    const string LoginGreetingsFormat = """
        💯 Hi, {0}! This chat application is powered by SignalR 👍🏽 ... Let's command some joke chatbots!
        <br>
        <br> <strong>Command format:</strong>
        <br> &nbsp; <pre><code>(joke|jokes)[:dad|chucknorris][:en (or another two letter locale i.e.; bg)]</code></pre>
        <br> <strong>Examples:</strong>
        <br> &nbsp; 1) typing "jokes:chucknorris:bg" will start the "Chuck Norris" chatbot, which will speak jokes continuously in Bulgarian.
        <br> &nbsp; 2) typing "joke" will start the "Dad" chatbot, and speak a single joke in English.
        <br>
        <br> <strong>Notes:</strong>
        <br> &nbsp;Anyone can command these and they are shared for all. Type "stop" to issue a global stop command. Finally, mix and match single or continuous joke(s), joke types and locales...
        """;

    string Username => Context?.User?.Identity?.Name ?? "Unknown";

    public ChatHub(ICommandSignalService commandSignal) => _commandSignal = commandSignal;

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.MessageReceived(
            new ActorMessage(
                "greeting", string.Format(LoginGreetingsFormat, Username), "👋", IsGreeting: true));

        await Clients.Others.UserLoggedOn(new Actor(Username));
    }

    public override async Task OnDisconnectedAsync(Exception? ex) =>
        await Clients.Others.UserLoggedOff(new Actor(Username));

    public async Task PostMessage(string message, string id = null!)
    {
        if (_commandSignal.IsRecognizedCommand(Username, message, out var command) &&
            command is not null)
        {
            await Clients.Caller.CommandSignalReceived(command);
            return;
        }

        await Clients.All.MessageReceived(
            new ActorMessage(UseOrCreateId(id), message, Username, IsEdit: id is not null));
    }

    public Task UserTyping(bool isTyping) =>
        Clients.Others.UserTyping(new ActorAction(Username, isTyping));

    static string UseOrCreateId(string id) =>
        string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
}