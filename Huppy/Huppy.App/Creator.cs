using Discord;
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

        public Creator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
            _appSettings = _serviceProvider.GetRequiredService<AppSettings>();
        }

        public async Task ConfigureCommandsAsync() =>
            await _serviceProvider.GetRequiredService<ICommandHandlerService>().InitializeAsync();

        public async Task CreateBot()
        {
            await _client.LoginAsync(Discord.TokenType.Bot, _appSettings.BotToken);
            await _client.StartAsync();

            await _client.SetGameAsync("Hello World!", null, Discord.ActivityType.Playing);
        }

        public async Task ConfigureClientEventsAsync()
        {
            _client.ShardReady += CreateSlashCommands;
        }

        private async Task CreateSlashCommands(DiscordSocketClient socketClient)
        {
            var guild = socketClient.GetGuild(0000000000000);

            var guildCommand = new SlashCommandBuilder();

            guildCommand.WithName(Ping.Name);
            guildCommand.WithDescription(Ping.Description);

            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (ApplicationCommandException exp)
            {
                var json = JsonConvert.SerializeObject(exp.Errors, Formatting.Indented);

                Log.Fatal(json);
            }
        }
    }
}