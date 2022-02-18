using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Entities;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.LoggerService;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Huppy.App.Configuration
{
    public class ModuleConfigurator
    {
        private readonly IServiceCollection _services;

        public ModuleConfigurator(IServiceCollection? services = null)
        {
            _services = services ?? new ServiceCollection();
        }

        public ModuleConfigurator AddAppSettings(AppSettings? settings = null)
        {
            settings ??= AppSettings.IsCreated
                ? AppSettings.Load()
                : AppSettings.Create();

            _services.AddSingleton(settings);

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
                MessageCacheSize = 1000
            });

            InteractionService interactionService = new(client);

            _services.AddSingleton(client)
                     .AddSingleton(interactionService)
                     .AddSingleton<ICommandHandlerService, CommandHandlerService>();

            return this;
        }

        public ModuleConfigurator AddServices()
        {
            return this;
        }

        public IServiceProvider Build() => _services.BuildServiceProvider();
    }
}