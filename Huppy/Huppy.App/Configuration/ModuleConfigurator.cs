using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Huppy.Core.Entities;
using Huppy.Core.Services.CommandService;
using Microsoft.Extensions.DependencyInjection;

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

        public ModuleConfigurator AddDiscord()
        {
            DiscordShardedClient client = new(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 1000
            });

            CommandService commandService = new(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false,
                ThrowOnError = false
            });

            _services.AddSingleton(client)
                     .AddSingleton(commandService)
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