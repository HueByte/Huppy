using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Attributes;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.HuppyCacheStorage;
using Huppy.Core.Services.Logger;
using Huppy.Core.Services.MiddlewareExecutor;
using Huppy.Core.Services.Paginator;
using Huppy.Infrastructure;
using Huppy.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Huppy.Core.Services.Huppy;

public class HuppyHostedService : IHostedService
{
    #region properties
    private readonly IJobManagerService _jobManager;
    private readonly DiscordShardedClient _client;
    private readonly AppSettings _appSettings;
    private readonly InteractionService _interactionService;
    private readonly ICommandHandlerService _commandHandler;
    private readonly LoggingService _loggingService;
    private readonly ILogger<HuppyHostedService> _logger;
    private readonly HuppyDbContext _dbContext;
    private readonly IServerInteractionService _serverInteractionService;
    private readonly CacheStorageService _cacheService;
    private readonly IPaginatorService _paginatorService;
    private readonly IEventLoopService _eventService;
    private readonly IReminderService _reminderService;
    private readonly MiddlewareExecutorService _middlewareExecutor;
    private readonly IAppMetadataService _appMetadataService;
    private bool isBotInitialized = false;

    #endregion
    public HuppyHostedService(DiscordShardedClient client, AppSettings appSettings, InteractionService interactionService, ICommandHandlerService commandHandlerService,
        LoggingService loggingService, ILogger<HuppyHostedService> logger, HuppyDbContext dbContext, IServerInteractionService serverInteractionService,
        CacheStorageService cacheService, IPaginatorService paginatorService, IEventLoopService eventService, IReminderService reminderService,
        MiddlewareExecutorService middlewareExecutorService, IAppMetadataService appMetadataService,
        IJobManagerService jobManagerService)
    {
        _client = client;
        _appSettings = appSettings;
        _interactionService = interactionService;
        _commandHandler = commandHandlerService;
        _loggingService = loggingService;
        _logger = logger;
        _dbContext = dbContext;
        _serverInteractionService = serverInteractionService;
        _cacheService = cacheService;
        _paginatorService = paginatorService;
        _eventService = eventService;
        _reminderService = reminderService;
        _middlewareExecutor = middlewareExecutorService;
        _appMetadataService = appMetadataService;
        _jobManager = jobManagerService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Huppy {version}", $"v{_appMetadataService.Version}");
        await CreateDatabase();
        await CreateCommands();
        await PopulateSingletons();
        await CreateEvents();
        await CreateBot();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Closing Huppy");
        Log.CloseAndFlush();
        return Task.CompletedTask;
    }

    public async Task CreateDatabase()
    {
        _logger.LogInformation("Verifying database");
        await _dbContext.Database.MigrateAsync();
    }

    public async Task CreateCommands()
    {
        _logger.LogInformation("Registering command modules");
        await _commandHandler.InitializeAsync();
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
        _interactionService.ComponentCommandExecuted += _commandHandler.ComponentExecuted;

        // logger events
        _client.Log += _loggingService.OnLogAsync;
        _interactionService.Log += _loggingService.OnLogAsync;

        // others
        _eventService.OnEventsRemoved += _reminderService.RemoveReminderRangeAsync;
        _middlewareExecutor.OnLogAsync += _loggingService.OnMiddlewareLog;

        return Task.CompletedTask;
    }

    public async Task OnShardFirstReady(DiscordSocketClient client)
    {
        if (isBotInitialized) return;

        _logger.LogInformation("Starting Job Manager");

        await _jobManager.StartEventLoop();
        await _jobManager.StartReminderJobs();
        await _jobManager.StartActivityControlJobs();
        _jobManager.MarkInitialized();

        isBotInitialized = true;
    }

    public async Task CreateBot()
    {
        _logger.LogInformation("Starting the bot");

        await _client.LoginAsync(TokenType.Bot, _appSettings.BotToken);

        await _client.StartAsync();

        await _client.SetGameAsync("Hello World!", null, Discord.ActivityType.Playing);
    }

    private async Task CreateSlashCommands(DiscordSocketClient socketClient)
    {
        if (isBotInitialized) return;

        _logger.LogInformation("Creating slash commands");

        try
        {
            // register commands to the global flow
            if (IsProd())
            {
                _logger.LogInformation("Registering commands globally");
                await _interactionService.RegisterCommandsGloballyAsync(true);
            }

            string[]? betaTestingGuildsTemp = _appSettings.BetaTestingGuilds?.Split(';').ToArray();
            ulong[] betaTestingGuilds = betaTestingGuildsTemp?.Length > 0 ? Array.ConvertAll(betaTestingGuildsTemp, UInt64.Parse) : Array.Empty<ulong>();

            string[]? debugGuildsTemp = _appSettings.DebugGuilds?.Split(';').ToArray()!;
            ulong[] debugGuilds = debugGuildsTemp?.Length > 0 ? Array.ConvertAll(debugGuildsTemp, UInt64.Parse) : Array.Empty<ulong>();

            (var debugModules, var debugCommands) = GetSpecialCommands<DebugCommandGroupAttribute, DebugCommandAttribute>();
            (var betaModules, var betaCommands) = GetSpecialCommands<BetaCommandGroupAttribute, BetaCommandAttribute>();

            foreach (var guildId in betaTestingGuilds.Union(debugGuilds))
            {
                try
                {
                    List<ModuleInfo> resultModules = new();
                    List<ICommandInfo> resultCommands = new();

                    // auto register commands to guilds 
                    if (!IsProd() && debugGuilds.Contains(guildId))
                    {
                        _logger.LogInformation("Registering commands for debug guilds with non prod environment to [{guild}]", guildId);
                        await _interactionService.RegisterCommandsToGuildAsync(guildId, true);
                    }
                    else
                    {
                        // clear guild registered modules
                        _logger.LogInformation("Clearing guild scoped commands of [{guild}]", guildId);
                        await _interactionService.AddModulesToGuildAsync(guildId, true, resultModules.ToArray());
                    }

                    if (betaTestingGuilds.Contains(guildId))
                    {
                        resultModules.AddRange(betaModules);
                        resultCommands.AddRange(betaCommands);
                    }

                    if (debugGuilds.Contains(guildId))
                    {
                        resultModules = resultModules.Union(debugModules).ToList();
                        resultCommands = resultCommands.Union(debugCommands).ToList();
                    }

                    _logger.LogInformation("Registering {privateCount} private modules to [{id}]", resultModules.Count, guildId);

                    // append special modules
                    await _interactionService.AddModulesToGuildAsync(guildId, false, resultModules.ToArray());

                    // disabled as for now since [DontAutoRegister] attribute works only for classes (for groups)
                    // so it cannot be applied for individual commands 
                    // await _interactionService.AddCommandsToGuildAsync(guildId, true, resultCommands.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.LogError("Server command logging error", ex);
                }
            }
        }
        catch (Exception exp)
        {
            _logger.LogError("Init error", exp);
        }
    }

    private Tuple<ModuleInfo[], ICommandInfo[]> GetSpecialCommands<TGroup, TMethod>()
    {
        // get command groups with TGroup attribute
        var commandModules = _interactionService.Modules
            .Where(e => e.Attributes.Any(e => e is TGroup))
            .ToArray();

        // get top level commands with TMethod attribute
        // var topLevelCommands = _interactionService.SlashCommands
        //     .Where(command => command.Attributes.Any(attrib => attrib is TMethod && command.IsTopLevelCommand))
        //     .Select(command => (CommandInfo<SlashCommandParameterInfo>)command)
        //     .ToArray();

        // get individual group commands from modules that hasn't got TGroup attribute, and have TMethod attribute 
        var moduleCommands = _interactionService.Modules
            .Where(module => !commandModules.Contains(module))
            .SelectMany(module => module.SlashCommands)
            .Where(command => command.Attributes.Any(attrib => attrib is TMethod))
            .ToArray();

        // connect individual module commands and top level commands for bulk insert
        // var commandInfoResult = topLevelCommands.Union(moduleCommands).ToArray();

        return new Tuple<ModuleInfo[], ICommandInfo[]>(commandModules, moduleCommands);
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