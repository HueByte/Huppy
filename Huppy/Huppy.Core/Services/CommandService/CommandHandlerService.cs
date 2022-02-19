using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace Huppy.Core.Services.CommandService
{
    public class CommandHandlerService : ICommandHandlerService
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppSettings _appSettings;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        public CommandHandlerService(DiscordShardedClient client, InteractionService interactionService, IServiceProvider serviceProvider, AppSettings appSettings, ILogger<CommandHandlerService> logger)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task InitializeAsync() =>
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        public async Task SlashCommandExecuted(SlashCommandInfo commandInfo, Discord.IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
            {
                _logger.LogInformation("Command [{CommandName}] executed for [{Username}] on [{GuildName}]", commandInfo.Name, context.User.Username, context.Guild.Name);
                return;
            }
            else
            {
                _logger.LogError("Command [{CommandName}] resulted in error: [{Error}]", commandInfo.Name, result.ErrorReason);
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Content = $"Unmet Precondition: {result.ErrorReason}");
                        break;

                    case InteractionCommandError.UnknownCommand:
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Content = "Unknown command");
                        break;

                    case InteractionCommandError.BadArgs:
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Content = "Invalid number or arguments");
                        break;

                    case InteractionCommandError.Exception:
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Content = $"Command exception:{result.ErrorReason}");
                        break;

                    case InteractionCommandError.Unsuccessful:
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Content = "Command could not be executed");
                        break;

                    default:
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Content = "Something went wrong");
                        break;
                }
            }
        }

        public async Task HandleCommandAsync(SocketInteraction command)
        {
            try
            {
                await command.DeferAsync();

                var ctx = new ShardedInteractionContext(_client, command);

                await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                await command.ModifyOriginalResponseAsync((msg) => msg.Content = "Something went wrong");
                throw;
            }
        }
    }
}