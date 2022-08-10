using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Attributes;
using Huppy.Core.Common.Constants;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Huppy.Core.Services.CommandService
{
    public class CommandHandlerService : ICommandHandlerService
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly CacheService _cacheService;
        private readonly IServiceScopeFactory _serviceFactory;
        private HashSet<string> _ephemeralCommands;
        private readonly ConcurrentDictionary<ulong, ulong> _executionTimes = new();
        public CommandHandlerService(DiscordShardedClient client, InteractionService interactionService, IServiceProvider serviceProvider, ILogger<CommandHandlerService> logger, CacheService cacheService, IServiceScopeFactory serviceFactory)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cacheService = cacheService;
            _serviceFactory = serviceFactory;
            _ephemeralCommands = new();
        }

        public async Task InitializeAsync()
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _ephemeralCommands = GetEphemeralCommands();
        }

        private HashSet<string> GetEphemeralCommands()
        {
            _logger.LogInformation("Registering ephemeral commands");

            // get Huppy assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.FullName!.StartsWith("Huppy"));

            List<string> methodNames = new();
            foreach (var asm in assemblies)
            {
                // search thru types that are not interface and inherit InteractionModuleBase<ShardedInteractionContext>, 
                // extract method info from them and then check for EphemeralAtrribute and SlashCommandAttribute,
                // if contains both of them, the command name is added to ephemeral hashset of names
                var methods = asm.GetTypes()
                    .Where(type => !type.IsInterface && typeof(InteractionModuleBase<ShardedInteractionContext>).IsAssignableFrom(type))
                    .SelectMany(t => t.GetMethods())
                    .Where(method => method.GetCustomAttributes(typeof(EphemeralAttribute), false).Length > 0 && method.GetCustomAttributes(typeof(SlashCommandAttribute), false).Length > 0)
                    .Select(method => (method.GetCustomAttribute(typeof(SlashCommandAttribute), false) as SlashCommandAttribute)?.Name)
                    .ToList();

                if (methods is not null) methodNames.AddRange(methods!);
            }

            return methodNames.ToHashSet<string>();
        }

        private bool CheckIfEphemeral(SocketSlashCommand command)
        {
            if (command is null || command.Data is null) return false;

            if (!(command.Data.Options.Count > 0))
                return _ephemeralCommands.Contains(command.CommandName);

            var subCommand = command.Data.Options.First();

            var name = subCommand.Type == ApplicationCommandOptionType.SubCommand
                ? subCommand.Name
                : command.Data.Name;

            if (!string.IsNullOrEmpty(name))
                return _ephemeralCommands.Contains(name);

            return false;
        }

        public async Task HandleCommandAsync(SocketInteraction command)
        {
            long startTime = Stopwatch.GetTimestamp();
            using var scope = _serviceFactory.CreateAsyncScope();
            var commandRepository = scope.ServiceProvider.GetRequiredService<ICommandLogRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            try
            {
                bool useEphemeral = CheckIfEphemeral((command as SocketSlashCommand)!);
                await command.DeferAsync(useEphemeral);

                // cache keeps all user IDs existing in database, if user is not present, he shall be added
                if (!_cacheService.CacheUsers.ContainsKey(command.User.Id))
                {
                    _logger.LogInformation("Adding new user [{Username}] to database", command.User.Username);

                    User user = new()
                    {
                        Id = command.User.Id,
                        Username = command.User.Username,
                        JoinDate = DateTime.UtcNow,
                    };

                    await userRepository.AddAsync(user);
                    await _cacheService.AddCacheUser(command.User.Id, command.User.Username);
                }

                var ctx = new ShardedInteractionContext(_client, command);
                await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError("Command execution error", ex);

                await command.ModifyOriginalResponseAsync((msg) => msg.Content = "Something went wrong");
                throw;
            }

            long endTime = Stopwatch.GetTimestamp();
            _executionTimes.TryAdd(command.Id, (ulong)((endTime - startTime) / 10_000));
        }

        public async Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
        {
            using var scope = _serviceFactory.CreateAsyncScope();
            var commandRepository = scope.ServiceProvider.GetRequiredService<ICommandLogRepository>();
            _executionTimes.Remove(context.Interaction.Id, out var miliseconds);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Command [{CommandName}] executed for [{Username}] in [{GuildName}] [{time} ms]", commandInfo.Name, context.User.Username, context.Guild.Name, miliseconds);
                return;
            }
            else
            {
                var embed = new EmbedBuilder().WithCurrentTimestamp()
                                              .WithColor(Color.Red)
                                              .WithThumbnailUrl(Icons.Error);

                _logger.LogError("Command [{CommandName}] resulted in error: [{Error}]", commandInfo.Name, result.ErrorReason);

                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        embed.WithTitle("Unmet Precondition");
                        embed.WithDescription(result.ErrorReason);
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    case InteractionCommandError.UnknownCommand:
                        embed.WithTitle("Unknown command");
                        embed.WithDescription(result.ErrorReason);
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    case InteractionCommandError.BadArgs:
                        embed.WithTitle($"Invalid number or arguments");
                        embed.WithDescription(result.ErrorReason);
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    case InteractionCommandError.Exception:
                        embed.WithTitle("Command exception");
                        embed.WithDescription(result.ErrorReason);
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    case InteractionCommandError.Unsuccessful:
                        embed.WithTitle("Command could not be executed");
                        embed.WithDescription(result.ErrorReason);
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    default:
                        embed.WithTitle("Something went wrong");
                        embed.WithDescription(result.ErrorReason);
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;
                }
            }

            CommandLog log = new()
            {
                CommandName = commandInfo.Name,
                Date = DateTime.UtcNow,
                IsSuccess = result.IsSuccess,
                UserId = context.User.Id
            };

            await commandRepository.AddAsync(log);
        }

        public async Task ComponentExecuted(SocketMessageComponent component)
        {
            using var scope = _serviceFactory.CreateAsyncScope();
            var commandRepository = scope.ServiceProvider.GetRequiredService<ICommandLogRepository>();
            _executionTimes.Remove(component.Id, out var miliseconds);

            _logger.LogInformation("Component [{dataid}] used by [{username}] [{time} ms]", component.Data.CustomId, component.User.Username, miliseconds);

            CommandLog log = new()
            {
                CommandName = component.Data.CustomId,
                Date = DateTime.UtcNow,
                IsSuccess = component.IsValidToken,
                UserId = component.User.Id
            };

            await commandRepository.AddAsync(log);
        }
    }
}