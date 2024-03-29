using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Attributes;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.MiddlewareExecutor;
using Huppy.Kernel;
using Huppy.Kernel.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.CommandHandler
{
    public class CommandHandlerService : ICommandHandlerService
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceFactory;
        private HashSet<string> _ephemeralCommands;
        private readonly MiddlewareExecutorService _middlewareExecutor;
        public CommandHandlerService(DiscordShardedClient client, InteractionService interactionService, IServiceProvider serviceProvider, ILogger<CommandHandlerService> logger, IServiceScopeFactory serviceFactory, MiddlewareExecutorService middlewareExecutor)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _serviceFactory = serviceFactory;
            _ephemeralCommands = new();
            _middlewareExecutor = middlewareExecutor;
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
                    .Where(type => !type.IsInterface && typeof(InteractionModuleBase<ExtendedShardedInteractionContext>).IsAssignableFrom(type))
                    .SelectMany(t => t.GetMethods())
                    .Where(method => method.GetCustomAttributes(typeof(EphemeralAttribute), false).Length > 0 && method.GetCustomAttributes(typeof(SlashCommandAttribute), false).Length > 0)
                    .Select(method => (method.GetCustomAttribute(typeof(SlashCommandAttribute), false) as SlashCommandAttribute)?.Name)
                    .ToList();

                if (methods is not null) methodNames.AddRange(methods!);
            }

            return methodNames.ToHashSet();
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
            try
            {
                bool useEphemeral = CheckIfEphemeral((command as SocketSlashCommand)!);
                await command.DeferAsync(useEphemeral);

                // Disposed in ExtendedShardedInteractionContext
                var scope = _serviceFactory.CreateAsyncScope();
                var ctx = new ExtendedShardedInteractionContext(_client, command, scope);

                await _middlewareExecutor.ExecuteBeforeAsync(scope, ctx);
                await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError("Command execution error", ex);
                await command.ModifyOriginalResponseAsync((msg) => msg.Content = "Something went wrong");

                // TODO: uncomment to rethrow for future global error handler
                // throw;
            }
        }

        public async Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
        {
            var extendedContext = context as ExtendedShardedInteractionContext;
            var scope = extendedContext is not null ? extendedContext.AsyncScope : _serviceFactory.CreateAsyncScope();

            try
            {
                if (!result.IsSuccess)
                {
                    var embed = new EmbedBuilder().WithCurrentTimestamp()
                                                  .WithColor(Color.Red)
                                                  .WithThumbnailUrl(Icons.Error);
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                            embed.WithTitle("Unmet Precondition");
                            embed.WithDescription(result.ErrorReason);
                            break;

                        case InteractionCommandError.UnknownCommand:
                            embed.WithTitle("Unknown command");
                            embed.WithDescription(result.ErrorReason);
                            break;

                        case InteractionCommandError.BadArgs:
                            embed.WithTitle($"Invalid number or arguments");
                            embed.WithDescription(result.ErrorReason);
                            break;

                        case InteractionCommandError.Exception:
                            embed.WithTitle("Command exception");
                            embed.WithDescription(result.ErrorReason);
                            break;

                        case InteractionCommandError.Unsuccessful:
                            embed.WithTitle("Command could not be executed");
                            embed.WithDescription(result.ErrorReason);
                            break;

                        default:
                            embed.WithTitle("Something went wrong");
                            embed.WithDescription(result.ErrorReason);
                            break;
                    }

                    await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                }

                await _middlewareExecutor.ExecuteAfterAsync(scope, commandInfo, extendedContext!, result);
            }
            catch (Exception) { }
            finally
            {
                if (extendedContext is null) await scope.DisposeAsync();
                else await extendedContext.DisposeAsync();
            }
        }

        public async Task ComponentExecuted(ComponentCommandInfo commandInfo, IInteractionContext context, IResult result)
        {
            var extendedContext = context as ExtendedShardedInteractionContext;
            var scope = extendedContext is not null ? extendedContext.AsyncScope : _serviceFactory.CreateAsyncScope();

            try
            {
                await _middlewareExecutor.ExecuteAfterAsync(scope, commandInfo, extendedContext!, result);
            }
            catch (Exception) { }
            finally
            {
                if (extendedContext is null) await scope.DisposeAsync();
                else await extendedContext.DisposeAsync();
            }
        }
    }
}