using System.Windows.Input;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Huppy.Core.Common.SlashCommands;
using Huppy.Core.Entities;
using Huppy.Core.Services.CommandService;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace Huppy.App
{
    public class Creator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordShardedClient _client;
        private readonly AppSettings _appSettings;
        private readonly InteractionService _interactionService;
        private readonly ICommandHandlerService _commandHandler;

        public Creator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
            _appSettings = _serviceProvider.GetRequiredService<AppSettings>();
            _interactionService = _serviceProvider.GetRequiredService<InteractionService>();
            _commandHandler = _serviceProvider.GetRequiredService<ICommandHandlerService>();
        }

        public async Task CreateCommands() =>
            await _serviceProvider.GetRequiredService<ICommandHandlerService>().InitializeAsync();

        public async Task CreateBot()
        {
            await _client.LoginAsync(TokenType.Bot, _appSettings.BotToken);

            await _client.StartAsync();

            await _client.SetGameAsync("Hello World!", null, Discord.ActivityType.Playing);
        }

        public async Task CreateEvents()
        {
            // sharded client events
            _client.ShardReady += CreateSlashCommands;
            _client.InteractionCreated += _commandHandler.HandleCommandAsync;

            // interaction service events
            _interactionService.SlashCommandExecuted += _commandHandler.SlashCommandExecuted;
        }

        private async Task CreateSlashCommands(DiscordSocketClient socketClient)
        {
            try
            {
                await _interactionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(_appSettings.HomeGuild));
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
            }
        }
    }
}