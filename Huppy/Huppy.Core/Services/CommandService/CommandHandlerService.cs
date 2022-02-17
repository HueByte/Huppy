using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Huppy.Core.Entities;
using Newtonsoft.Json;
using Serilog;

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

            _client.SlashCommandExecuted += HandleCommandAsync;
        }

        public async Task InitializeAsync() => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        public async Task HandleCommandAsync(SocketSlashCommand command)
        {
            await command.DeferAsync();
            _ = Task.Run(() => Log.Information(JsonConvert.SerializeObject(command.Data)));
            await command.ModifyOriginalResponseAsync(async (msg) =>
            {
                msg.Content = "pong";
            });
            // await command.ModifyOriginalResponseAsync($"Executed {command.Data.Name}");

            // var context = new ShardedCommandContext(_client, command);
        }
    }
}