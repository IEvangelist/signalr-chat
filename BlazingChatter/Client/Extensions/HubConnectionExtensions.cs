using BlazingChatter.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace BlazingChatter.Client.Extensions
{
    public static class HubConnectionExtensions
    {
        public static IDisposable OnMessageReceived(
            this HubConnection connection, Func<ActorMessage, Task> handler) =>
            connection.On("MessageReceived", handler);

        public static IDisposable OnUserLoggedOn(
            this HubConnection connection, Func<Actor, Task> handler) =>
            connection.On("UserLoggedOn", handler);

        public static IDisposable OnUserLoggedOff(
            this HubConnection connection, Func<Actor, Task> handler) =>
            connection.On("UserLoggedOff", handler);

        public static IDisposable OnUserTyping(
            this HubConnection connection, Func<ActorAction, Task> handler) =>
            connection.On("UserTyping", handler);

        public static IDisposable OnCommandSignalReceived(
            this HubConnection connection, Action<ActorCommand> handler) =>
            connection.On("CommandSignalReceived", handler);
    }
}
