using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
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
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        public CommandHandlerService(DiscordShardedClient client, InteractionService interactionService, IServiceProvider serviceProvider, ILogger<CommandHandlerService> logger)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
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
                var embed = new EmbedBuilder().WithCurrentTimestamp()
                                              .WithColor(Color.Red)
                                              .WithThumbnailUrl(Icons.Error);

                _logger.LogError("Command [{CommandName}] resulted in error: [{Error}]", commandInfo.Name, result.ErrorReason);
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        embed.WithTitle($"Unmet Precondition: {result.ErrorReason}");
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    case InteractionCommandError.UnknownCommand:
                        embed.WithTitle("Unknown command");
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    case InteractionCommandError.BadArgs:
                        embed.WithTitle($"Invalid number or arguments");
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    case InteractionCommandError.Exception:
                        embed.WithTitle($"Command exception:{result.ErrorReason}");
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    case InteractionCommandError.Unsuccessful:
                        embed.WithTitle("Command could not be executed");
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
                        break;

                    default:
                        embed.WithTitle("Something went wrong");
                        await context.Interaction.ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
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