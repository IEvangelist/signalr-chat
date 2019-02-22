using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Hubs
{
    public class ChatHub : Hub
    {
        string Username => Context.User.Identity.Name;

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync(
                "MessageReceived",
                new
                {
                    text = $"💯 Hi, {Username}! This chat application is powered by SignalR 👍🏽",
                    id = "greeting",
                    isGreeting = true,
                    user = "👋"
                });

            await Clients.Others.SendAsync(
                "UserLoggedOn", new
                {
                    user = Username
                });
        }

        [Authorize]
        public async Task PostMessage(string message, string id = null)
            => await Clients.All.SendAsync(
                "MessageReceived",
                new
                {
                    text = message,
                    id = UseOrCreateId(id),
                    isEdit = id != null,
                    user = Username
                });

        [Authorize]
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
}