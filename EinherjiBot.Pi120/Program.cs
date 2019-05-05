﻿using System;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot
{
    class Program
    {
        private static BotInitializer _initializer;

        static async Task Main(string[] args)
        {
            _initializer = new BotInitializer();
            await _initializer.StartClient();
            _initializer.Client.Connected += Client_Connected;
            await Task.Delay(-1);
        }

        private static Task Client_Connected()
        {
            return Task.CompletedTask;
        }
    }
}
