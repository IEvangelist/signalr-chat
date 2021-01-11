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
        string[] _usersTyping;
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

            await _hubConnection.InvokeAsync("UserTyping", _isTyping = isTyping);
        }

        async Task AppendToMessage(string text)
        {
            _message += text;

            await SetIsTyping(false);
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
