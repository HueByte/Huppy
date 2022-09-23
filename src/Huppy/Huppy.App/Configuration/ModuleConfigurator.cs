using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.App.Middlewares;
using Huppy.Core.Extensions;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.Activity;
using Huppy.Core.Services.AiStabilizer;
using Huppy.Core.Services.App;
using Huppy.Core.Services.CommandHandler;
using Huppy.Core.Services.CommandLog;
using Huppy.Core.Services.EventLoop;
using Huppy.Core.Services.GPT;
using Huppy.Core.Services.HuppyCacheStorage;
using Huppy.Core.Services.JobManager;
using Huppy.Core.Services.Logger;
using Huppy.Core.Services.MiddlewareExecutor;
using Huppy.Core.Services.Paginator;
using Huppy.Core.Services.Reminder;
using Huppy.Core.Services.Resources;
using Huppy.Core.Services.ScopedData;
using Huppy.Core.Services.ServerInteraction;
using Huppy.Core.Services.Ticket;
using Huppy.Core.Services.TimedEvents;
using Huppy.Core.Services.Urban;
using Huppy.Infrastructure;
using Huppy.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Huppy.App.Configuration
{
    public class ModuleConfigurator
    {
        private readonly IServiceCollection _services;
        private AppSettings? _appSettings;

        public ModuleConfigurator(IServiceCollection? services = null)
        {
            _services = services ?? new ServiceCollection();
        }

        public ModuleConfigurator AddAppSettings(AppSettings? settings = null)
        {
            _appSettings = settings ?? (AppSettings.IsCreated
                ? AppSettings.Load()
                : AppSettings.Create());

            _services.AddSingleton(_appSettings);

            return this;
        }

        public ModuleConfigurator AddGRPCServices()
        {
            _services.AddGrpcClient<GPTProto.GPTProtoClient>((services, options) =>
            {
                //var basketApi = services.GetRequiredService<IOptions<UrlsConfig>>().Value.HuppyCoreUrl;
                options.Address = new Uri(_appSettings?.Microservices?.HuppyCoreUrl!);
            });

            _services.AddGrpcClient<CommandLogProto.CommandLogProtoClient>((services, options) =>
            {
                options.Address = new Uri(_appSettings?.Microservices?.HuppyCoreUrl!);
            });

            return this;
        }

        public ModuleConfigurator AddLogger(ILogger logger)
        {
            _services.AddLogging(conf => conf.AddSerilog(logger));
            _services.AddSingleton<LoggingService>();

            return this;
        }

        public ModuleConfigurator AddMiddlewares()
        {
            MiddlewareExecutorService middlewareExecutor = new();
            _services.UseMiddleware<ScopedDataMiddleware>(middlewareExecutor);
            _services.UseMiddleware<DataSynchronizationMiddleware>(middlewareExecutor);
            _services.UseMiddleware<CommandLogMiddleware>(middlewareExecutor);

            middlewareExecutor.FillMissingMiddlewares();

            _services.AddSingleton(middlewareExecutor);

            return this;
        }

        public ModuleConfigurator AddDiscord()
        {
            DiscordShardedClient client = new(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 100,
                GatewayIntents = GatewayIntents.Guilds
                    | GatewayIntents.GuildBans
                    | GatewayIntents.GuildEmojis
                    | GatewayIntents.GuildIntegrations
                    | GatewayIntents.GuildWebhooks
                    | GatewayIntents.GuildInvites
                    | GatewayIntents.GuildVoiceStates
                    | GatewayIntents.GuildMessageReactions
                    | GatewayIntents.DirectMessageReactions
                    | GatewayIntents.GuildScheduledEvents
                    | GatewayIntents.GuildMembers
            });

            InteractionServiceConfig interactionServiceConfig = new()
            {
                AutoServiceScopes = false,
                DefaultRunMode = RunMode.Async,
            };

            InteractionService interactionService = new(client, interactionServiceConfig);

            _services.AddSingleton(client)
                     .AddSingleton(interactionService)
                     .AddSingleton<ICommandHandlerService, CommandHandlerService>();

            return this;
        }

        public ModuleConfigurator AddDatabase()
        {
            _services.AddDbContext(_appSettings?.ConnectionString!);

            return this;
        }

        public ModuleConfigurator AddServices()
        {
            // singleton services
            _services.AddSingleton<CacheStorageService>();
            _services.AddSingleton<IAiStabilizerService, AiStabilizerService>();
            _services.AddSingleton<IServerInteractionService, ServerInteractionService>();
            _services.AddSingleton<ITimedEventsService, TimedEventsService>();
            _services.AddSingleton<IPaginatorService, PaginatorService>();
            _services.AddSingleton<IEventLoopService, EventLoopService>();
            _services.AddSingleton<IActivityControlService, ActivityControlService>();
            _services.AddSingleton<IAppMetadataService, AppMetadataService>();
            _services.AddSingleton<IJobManagerService, JobManagerService>();

            // scoped services
            _services.AddScoped<IReminderService, ReminderService>();
            _services.AddScoped<ITicketService, TicketService>();
            _services.AddScoped<IScopedDataService, ScopedDataService>();
            _services.AddScoped<IResourcesService, ResourcesService>();
            _services.AddScoped<ICommandLogService, CommandLogService>();

            // http clients
            _services.AddScoped<IGPTService, GPTService>();
            _services.AddScoped<IUrbanService, UrbanService>();

            // repositories
            _services.AddScoped<IUserRepository, UserRepository>();
            _services.AddScoped<IServerRepository, ServerRepository>();
            _services.AddScoped<IReminderRepository, ReminderRepository>();
            _services.AddScoped<ITicketRepository, TicketRepository>();

            return this;
        }

        public ModuleConfigurator AddHttpClients()
        {
            _services.AddHttpClient("GPT", httpclient =>
            {
                httpclient.BaseAddress = new Uri(_appSettings?.GPT!.BaseUrl!);
                httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_appSettings?.GPT!.ApiKey}");

                if (!string.IsNullOrEmpty(_appSettings?.GPT?.Orgranization))
                    httpclient.DefaultRequestHeaders.Add("OpenAI-Organization", _appSettings?.GPT!.Orgranization);
            });

            _services.AddHttpClient("Urban", httpClient =>
            {
                httpClient.BaseAddress = new Uri(_appSettings?.UrbanApi!.BaseUrl!);
                httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _appSettings?.UrbanApi!.Host);
                httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _appSettings?.UrbanApi!.Key);
            });

            return this;
        }
    }
}