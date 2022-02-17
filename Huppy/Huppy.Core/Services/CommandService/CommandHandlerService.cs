using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Huppy.Core.Entities;

namespace Huppy.Core.Services.CommandService
{
    public class CommandHandlerService : ICommandHandlerService
    {
        private readonly DiscordShardedClient _client;
        private readonly Discord.Commands.CommandService _commands;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppSettings _appSettings;
        public CommandHandlerService(DiscordShardedClient client, Discord.Commands.CommandService commands, IServiceProvider serviceProvider, AppSettings appSettings)
        {
            _client = client;
            _commands = commands;
            _serviceProvider = serviceProvider;
            _appSettings = appSettings;

            _client.MessageReceived += HandleCommandAsync;
        }

        public async Task InitializeAsync() => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        public async Task HandleCommandAsync(SocketMessage message)
        {

        }
    }
}