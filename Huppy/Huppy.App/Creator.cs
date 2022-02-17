using Discord.WebSocket;
using Huppy.Core.Entities;
using Huppy.Core.Services.CommandService;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.App
{
    public class Creator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordShardedClient _client;
        private readonly AppSettings _appSettings;

        public Creator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
        }

        public async Task ConfigureCommandsAsync() =>
            await _serviceProvider.GetRequiredService<CommandHandlerService>().InitializeAsync();

        public async Task CreateBot()
        {
            await _client.LoginAsync(Discord.TokenType.Bot, _appSettings.BotToken);
            await _client.StartAsync();

            await _client.SetGameAsync("Hello World!", null, Discord.ActivityType.Playing);
        }

        public async Task ConfigureClientEventsAsync()
        {
            // _client.ShardReady 
        }
    }
}