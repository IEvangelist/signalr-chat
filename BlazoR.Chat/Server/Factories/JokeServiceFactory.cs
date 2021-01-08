﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlazorR.Chat.Enums;
using BlazorR.Chat.Services;

namespace BlazorR.Chat.Factories
{
    public class JokeServiceFactory : IJokeServiceFactory
    {
        readonly IEnumerable<IJokeService> _jokeServices;

        public JokeServiceFactory(IEnumerable<IJokeService> jokeServices) =>
            _jokeServices = jokeServices;

        IJokeService IJokeServiceFactory.Get(JokeType type) =>
            type switch
            {
                JokeType.Dad => _jokeServices.SingleOrDefault(svc => svc is DadJokeService),
                JokeType.ChuckNorris => _jokeServices.SingleOrDefault(svc => svc is ChuckNorrisJokeService),

                _ => throw new ArgumentException("This is not funny, no jokes for you...", nameof(type))
            };
    }
}