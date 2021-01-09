using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using BlazingChatter.Services;

namespace BlazingChatter.Hubs
{
    [Authorize]
    public class ChatHub : Hub // <IChatHub>
    {
        readonly ICommandSignalService _commandSignal;

        const string LoginGreetingsFormat =
@"💯 Hi, {0}! This chat application is powered by SignalR 👍🏽 ... Let's command some joke chatbots!
<br>
<br> <strong>Command format:</strong>
<br> &nbsp; <pre><code>(joke|jokes)[:dad|chucknorris][:en (or another two letter locale i.e.; bg)]</code></pre>
<br> <strong>Examples:</strong>
<br> &nbsp; 1) typing ""jokes:chucknorris:bg"" will start the ""Chuck Norris"" chatbot, which will speak jokes continously in Bulgarian.
<br> &nbsp; 2) typing ""joke"" will start the ""Dad"" chatbot, and speak a single joke in English.
<br>
<br> <strong>Notes:</strong>
<br> &nbsp;Anyone can command these and they are shared for all. Type ""stop"" to issue a global stop command. Finally, mix and match single or continous joke(s), joke types and locales...";

        string Username => Context.User.Identity.Name;

        public ChatHub(ICommandSignalService commandSignal) => _commandSignal = commandSignal;

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync(
                "MessageReceived",
                new
                {
                    text = string.Format(LoginGreetingsFormat, Username),
                    id = "greeting",
                    isGreeting = true,
                    user = "👋"
                });

            await Clients.Others.SendAsync(
                "UserLoggedOn", 
                new
                {
                    user = Username
                });
        }

        public override async Task OnDisconnectedAsync(Exception ex) 
            => await Clients.Others.SendAsync(
                "UserLoggedOff",
                new
                {
                    user = Username
                });

        public async Task PostMessage(string message, string id = null)
        {
            if (_commandSignal.IsRecognizedCommand(message))
            {
                return;
            }

            await Clients.All.SendAsync(
                "MessageReceived",
                new
                {
                    text = message,
                    id = UseOrCreateId(id),
                    isEdit = id != null,
                    user = Username
                });
        }

        public async Task UserTyping(bool isTyping)
            => await Clients.Others.SendAsync(
                "UserTyping",
                new
                {
                    isTyping,
                    user = Username
                });

        static string UseOrCreateId(string id)
            => string.IsNullOrWhiteSpace(id)
                ? Guid.NewGuid().ToString()
                : id;
    }

    public interface IChatHub
    {
        Task UserLoggedOn(object args);

        Task UserLoggedOff(object args);

        Task UserTyping(object args);

        Task MessageReceived(object args);
    }

    public class StronglyTypedChatHub : Hub<IChatHub>
    {
        string Username => Context.User.Identity.Name;

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.MessageReceived(
                new
                {
                    text = $"💯 Hi, {Username}! This chat application is powered by SignalR 👍🏽",
                    id = "greeting",
                    isGreeting = true,
                    user = "👋"
                });

            await Clients.Others.UserLoggedOn(
                new
                {
                    user = Username
                });
        }

        public override async Task OnDisconnectedAsync(Exception ex)
            => await Clients.Others.UserLoggedOff(
                new
                {
                    user = Username
                });

        public async Task PostMessage(string message, string id = null)
            => await Clients.All.MessageReceived(
                new
                {
                    text = message,
                    id = UseOrCreateId(id),
                    isEdit = id != null,
                    user = Username
                });

        public async Task UserTyping(bool isTyping)
            => await Clients.Others.UserTyping(
                new
                {
                    isTyping,
                    user = Username
                });

        static string UseOrCreateId(string id)
            => string.IsNullOrWhiteSpace(id)
                ? Guid.NewGuid().ToString()
                : id;
    }
}