using System;
using System.Collections.Generic;
using System.Linq;
using BlazingChatter.Enums;
using BlazingChatter.Services;

namespace BlazingChatter.Factories
{
    public class JokeServiceFactory : IJokeServiceFactory
    {
        readonly IEnumerable<IJokeService> _jokeServices;

        public JokeServiceFactory(IEnumerable<IJokeService> jokeServices) =>
            _jokeServices = jokeServices;

        IJokeService IJokeServiceFactory.Get(JokeType type) =>
            type switch
            {
                JokeType.Dad => _jokeServices.OfType<DadJokeService>().Single(),
                JokeType.ChuckNorris => _jokeServices.OfType<ChuckNorrisJokeService>().Single(),

                _ => throw new ArgumentException("This is not funny, no jokes for you...", nameof(type))
            };
    }
}