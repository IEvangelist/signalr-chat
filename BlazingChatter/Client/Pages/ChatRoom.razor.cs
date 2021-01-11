using BlazingChatter.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazingChatter.Client.Pages
{
    public partial class ChatRoom
    {
        readonly Dictionary<string, ActorMessage> _messages = new(StringComparer.OrdinalIgnoreCase);
        
        HubConnection _hubConnection;
        string _messageId;
        string _message;
        string[] _typingUsers;
        bool _isTyping;

        [Parameter]
        public ClaimsPrincipal User { get; set; }

        [Inject]
        public NavigationManager Nav { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Nav.ToAbsoluteUri("/chat"))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<ActorMessage>("MessageReceived", OnMessageReceivedAsync);

            await _hubConnection.StartAsync();
        }

        async Task OnMessageReceivedAsync(ActorMessage message)
        {
            await InvokeAsync(() =>
            {
                _messages[message.Id] = message;

                StateHasChanged();
            });
        }

        bool OwnsMessage(string user) => User.Identity.Name == user;

        async Task StartEdit(ActorMessage message)
        {
            await InvokeAsync(() =>
            {
                _messageId = message.Id;
                _message = message.Text;

                StateHasChanged();
            });
        }
    }
}
