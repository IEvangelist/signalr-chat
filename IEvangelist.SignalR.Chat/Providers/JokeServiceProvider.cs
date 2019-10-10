using System;
using System.Collections.Generic;
using System.Linq;
using IEvangelist.SignalR.Chat.Enums;
using IEvangelist.SignalR.Chat.Services;

namespace IEvangelist.SignalR.Chat.Providers
{
    public class JokeServiceProvider : IJokeServiceProvider
    {
        readonly IEnumerable<IJokeService> _jokeServices;

        public JokeServiceProvider(IEnumerable<IJokeService> jokeServices) => _jokeServices = jokeServices;

        IJokeService IJokeServiceProvider.Get(JokeType type) =>
            type switch
            {
                JokeType.Dad => _jokeServices.SingleOrDefault(svc => svc is DadJokeService),
                JokeType.ChuckNorris => _jokeServices.SingleOrDefault(svc => svc is ChuckNorrisJokeService),

                _ => throw new Exception("WTF: no joke!")
            };
    }
}