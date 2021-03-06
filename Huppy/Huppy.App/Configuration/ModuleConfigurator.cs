using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.AiStabilizerService;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.EventService;
using Huppy.Core.Services.GPTService;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.LoggerService;
using Huppy.Core.Services.NewsService;
using Huppy.Core.Services.PaginatedEmbedService;
using Huppy.Core.Services.ServerInteractionService;
using Huppy.Core.Services.TimedEventsService;
using Huppy.Core.Services.UrbanService;
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

        public ModuleConfigurator AddLogger(ILogger logger)
        {
            _services.AddLogging(conf => conf.AddSerilog(logger));
            _services.AddSingleton<LoggingService>();

            return this;
        }

        public ModuleConfigurator AddDiscord()
        {
            DiscordShardedClient client = new(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 1000,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildBans
            });

            InteractionService interactionService = new(client);

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
            _services.AddSingleton<CacheService>();
            _services.AddSingleton<IAiStabilizerService, AiStabilizerService>();
            _services.AddSingleton<IServerInteractionService, ServerInteractionService>();
            _services.AddSingleton<ITimedEventsService, TimedEventsService>();
            _services.AddSingleton<IPaginatorEmbedService, PaginatorEmbedService>();
            _services.AddSingleton<IEventService, EventService>();

            _services.AddScoped<IGPTService, GPTService>();
            _services.AddScoped<IUrbanService, UrbanService>();
            _services.AddScoped<INewsApiService, NewsApiService>();

            _services.AddScoped<IUserRepository, UserRepository>();
            _services.AddScoped<IServerRepository, ServerRepository>();
            _services.AddScoped<ICommandLogRepository, CommandLogRepository>();

            return this;
        }

        public ModuleConfigurator AddHttpClient()
        {
            _services.AddHttpClient("GPT", httpclient =>
            {
                httpclient.BaseAddress = new Uri(_appSettings?.GPT!.BaseUrl!);
                httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_appSettings?.GPT!.ApiKey}");
                httpclient.DefaultRequestHeaders.Add("OpenAI-Organization", _appSettings?.GPT!.Orgranization);
            });

            _services.AddHttpClient("Urban", httpClient =>
            {
                httpClient.BaseAddress = new Uri(_appSettings?.UrbanApi!.BaseUrl!);
                httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _appSettings?.UrbanApi!.Host);
                httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _appSettings?.UrbanApi!.Key);
            });

            _services.AddHttpClient("News", httpClient =>
            {
                httpClient.BaseAddress = new Uri(_appSettings?.NewsAPI!.BaseUrl!);
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", _appSettings?.NewsAPI!.ApiKey);
            });

            return this;
        }

        public IServiceProvider Build() => _services.BuildServiceProvider();
    }
}