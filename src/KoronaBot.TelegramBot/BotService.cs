﻿using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace KoronaBot.TelegramBot
{
    class BotService : IHostedService
    {
        private readonly Client _client;

        public BotService(Client client)
        {
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.Stop();
        }
    }
}
