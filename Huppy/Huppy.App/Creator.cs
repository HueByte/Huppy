using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Attributes;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.EventService;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.LoggerService;
using Huppy.Core.Services.PaginatorService;
using Huppy.Core.Services.ReminderService;
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
        private readonly IPaginatorService _paginatorService;
        private readonly IEventService _eventService;
        private readonly IReminderService _reminderService;
        private bool isBotInitialized = false;

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
            _eventService = _serviceProvider.GetRequiredService<IEventService>();
            _paginatorService = _serviceProvider.GetRequiredService<IPaginatorService>();
            _reminderService = _serviceProvider.GetRequiredService<IReminderService>();
        }

        public async Task CreateDatabase()
        {
            _logger.LogInformation("Verifying database");
            await _dbContext.Database.MigrateAsync();
        }

        public async Task CreateCommands()
        {
            _logger.LogInformation("Registering command modules");
            await _serviceProvider.GetRequiredService<ICommandHandlerService>().InitializeAsync();
        }

        public async Task PopulateSingletons()
        {
            if (isBotInitialized) return;

            _logger.LogInformation("Populating singletons");
            await _cacheService.Initialize();

            BuildStaticEmbeds embedsBuilder = new(_interactionService);
            await _paginatorService.RegisterStaticEmbeds(embedsBuilder.GetStaticEmbeds());
        }

        public Task CreateEvents()
        {
            _logger.LogInformation("Creating events");

            // shard events
            _client.ShardReady += CreateSlashCommands;
            _client.ShardReady += _loggingService.OnReadyAsync;
            _client.ShardReady += OnShardFirstReady;

            // interaction event
            _client.UserJoined += _serverInteractionService.OnUserJoined;
            _client.JoinedGuild += _serverInteractionService.HuppyJoined;

            // command events
            _client.InteractionCreated += _commandHandler.HandleCommandAsync;
            _interactionService.SlashCommandExecuted += _commandHandler.SlashCommandExecuted;
            _client.ButtonExecuted += _commandHandler.ComponentExecuted;

            // logger events
            _client.Log += _loggingService.OnLogAsync;
            _interactionService.Log += _loggingService.OnLogAsync;

            // others
            _eventService.OnEventsRemoved += _reminderService.RemoveReminderRange;

            return Task.CompletedTask;
        }

        public async Task OnShardFirstReady(DiscordSocketClient client)
        {
            if (isBotInitialized) return;

            await StartTimedEvents();
            await CreateReminders();

            isBotInitialized = true;
        }

        public async Task CreateBot()
        {
            _logger.LogInformation("Starting the bot");

            await _client.LoginAsync(TokenType.Bot, _appSettings.BotToken);

            await _client.StartAsync();

            await _client.SetGameAsync("Hello World!", null, Discord.ActivityType.Playing);
        }

        public async Task StartTimedEvents()
        {
            if (isBotInitialized) return;

            await _timedEventService.StartTimers();
            _eventService.Initialize();
        }

        public async Task CreateReminders()
        {
            if (isBotInitialized) return;

            await _reminderService.Initialize();
        }

        private async Task CreateSlashCommands(DiscordSocketClient socketClient)
        {
            _logger.LogInformation("Creating slash commands");

            try
            {
                string[]? betaTestingGuildsTemp = _appSettings.BetaTestingGuilds?.Split(';').ToArray();
                ulong[] betaTestingGuilds = betaTestingGuildsTemp?.Length > 0 ? Array.ConvertAll(betaTestingGuildsTemp, UInt64.Parse) : Array.Empty<ulong>();

                string[]? debugGuildsTemp = _appSettings.DebugGuilds?.Split(';').ToArray()!;
                ulong[] debugGuilds = debugGuildsTemp?.Length > 0 ? Array.ConvertAll(debugGuildsTemp, UInt64.Parse) : Array.Empty<ulong>();

                var notAutoRegisteredCommandGroups = _interactionService.Modules
                    .Where(e => e.Attributes.Any(e => e is DebugGroupAttribute))
                    .ToList();

                _logger.LogInformation("Registering beta commands");
                foreach (var guild in betaTestingGuilds)
                {
                    _logger.LogInformation("Registering beta commands to [{guild}]", guild);
                    await _interactionService.RegisterCommandsToGuildAsync(guild);
                }

                // register commands to the global flow
                if (IsProd())
                {
                    _logger.LogInformation("Registering commands globally...");
                    if (!isBotInitialized) await _interactionService.RegisterCommandsGloballyAsync();
                }

                _logger.LogInformation("Registering debug commands");
                foreach (var guild in debugGuilds)
                {
                    await _interactionService.AddModulesToGuildAsync(guild, true, notAutoRegisteredCommandGroups.ToArray());
                }
            }
            catch (Exception exp)
            {
                Log.Error($"{exp.Message}\n{exp.StackTrace}");
            }
        }

        private static bool IsProd()
        {
#if DEBUG
            return false;
#else
            return true;
#endif
        }
    }
}