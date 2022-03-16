using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Entities;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.LoggerService;
using Huppy.Core.Services.ServerInteractionService;
using Huppy.Core.Services.TimedEventsService;
using Huppy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly LoggingService _loggingService;
        private readonly ILogger<Creator> _logger;
        private readonly HuppyDbContext _dbContext;
        private readonly IServerInteractionService _serverInteractionService;
        private readonly ITimedEventsService _timedEventService;
        private readonly CacheService _cacheService;

        public Creator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
            _appSettings = _serviceProvider.GetRequiredService<AppSettings>();
            _interactionService = _serviceProvider.GetRequiredService<InteractionService>();
            _commandHandler = _serviceProvider.GetRequiredService<ICommandHandlerService>();
            _loggingService = _serviceProvider.GetRequiredService<LoggingService>();
            _logger = _serviceProvider.GetRequiredService<ILogger<Creator>>();
            _dbContext = _serviceProvider.GetRequiredService<HuppyDbContext>();
            _serverInteractionService = _serviceProvider.GetRequiredService<IServerInteractionService>();
            _timedEventService = _serviceProvider.GetRequiredService<ITimedEventsService>();
            _cacheService = _serviceProvider.GetRequiredService<CacheService>();
        }

        public async Task CreateDatabase()
        {
            await _dbContext.Database.MigrateAsync();
        }

        public async Task PopulateSingletons()
        {
            await _cacheService.Initialize();
        }

        public async Task CreateCommands() =>
            await _serviceProvider.GetRequiredService<ICommandHandlerService>().InitializeAsync();

        public async Task CreateBot()
        {
            _logger.LogInformation("Starting the bot");

            await _client.LoginAsync(TokenType.Bot, _appSettings.BotToken);

            await _client.StartAsync();

            await _client.SetGameAsync("Hello World!", null, Discord.ActivityType.Playing);
        }

        public async Task CreateEvents()
        {
            _logger.LogInformation("Creating events");

            // sharded client events
            _client.ShardReady += CreateSlashCommands;
            _client.ShardReady += StartTimedEvents;
            _client.InteractionCreated += _commandHandler.HandleCommandAsync;
            _client.ButtonExecuted += _commandHandler.ComponentHandler;
            _client.Log += _loggingService.OnLogAsync;
            _client.ShardReady += _loggingService.OnReadyAsync;
            _client.UserJoined += _serverInteractionService.OnUserJoined;
            _client.JoinedGuild += _serverInteractionService.HuppyJoined;

            // interaction service events
            _interactionService.SlashCommandExecuted += _commandHandler.SlashCommandExecuted;
            _interactionService.Log += _loggingService.OnLogAsync;
        }

        public async Task StartTimedEvents(DiscordSocketClient socketClient)
        {
            await _timedEventService.StartTimers();
        }

        private async Task CreateSlashCommands(DiscordSocketClient socketClient)
        {
            _logger.LogInformation("Creating slash commands");

            try
            {
                string[] guilds = _appSettings.HomeGuilds!.Split(';').ToArray();
                ulong[] homeGuilds = Array.ConvertAll(guilds, UInt64.Parse);

                foreach (var guild in homeGuilds)
                {
                    _logger.LogInformation("Registering commands to [ {guild} ]", guild);
                    await _interactionService.RegisterCommandsToGuildAsync(guild);
                    await Task.Delay(1000);
                }
            }
            catch (Exception exp)
            {
                Log.Error($"{exp.Message}\n{exp.StackTrace}");
            }
        }
    }
}