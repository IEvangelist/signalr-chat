using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace IEvangelist.SignalR.Chat.Hubs
{
    public class ChatHub : Hub
    {
        string UserName => Context.User.Identity.Name;

        //[Authorize]
        public async Task PostMessage(string message, string id = null)
            => await Clients.All.SendAsync(
                "MessageReceived",                
                new
                {
                    text = message,
                    id = UseOrCreateId(id),
                    user = UserName
                });

        static string UseOrCreateId(string id)
            => string.IsNullOrWhiteSpace(id)
                ? Guid.NewGuid().ToString()
                : id;
    }
}