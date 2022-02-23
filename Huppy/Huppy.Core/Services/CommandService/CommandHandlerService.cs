using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Huppy.Core.Services.HuppyCacheService;
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
        private readonly ICommandLogRepository _commandRepository;
        private readonly IUserRepository _userRepository;
        private readonly CacheService _cacheService;
        public CommandHandlerService(DiscordShardedClient client, InteractionService interactionService, IServiceProvider serviceProvider, ILogger<CommandHandlerService> logger, ICommandLogRepository commandRepository, IUserRepository userRepository, CacheService cacheService)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _commandRepository = commandRepository;
            _userRepository = userRepository;
            _cacheService = cacheService;
        }

        public async Task InitializeAsync()
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        public async Task SlashCommandExecuted(SlashCommandInfo commandInfo, Discord.IInteractionContext context, IResult result)
        {
            CommandLog log = new()
            {
                CommandName = commandInfo.Name,
                Date = DateTime.UtcNow,
                IsSuccess = result.IsSuccess,
                UserId = context.User.Id
            };

            await _commandRepository.AddAsync(log);

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
            if (!_cacheService.UserBasicData.ContainsKey(command.User.Id))
            {
                _logger.LogInformation("Adding new user [{Username}] to database", command.User.Username);
                User user = new()
                {
                    Id = command.User.Id,
                    Username = command.User.Username,
                    JoinDate = DateTime.UtcNow,
                };

                await _userRepository.AddAsync(user);
                await _cacheService.AddBasicUserData(command.User.Id, command.User.Username);
            }

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

        public async Task ComponentHandler(SocketMessageComponent component)
        {
            _logger.LogInformation("component [{name}] with info {0}, {1}", component.Id, component.Message, component.Data.CustomId);
            CommandLog log = new()
            {
                CommandName = component.Data.CustomId,
                Date = DateTime.UtcNow,
                IsSuccess = component.IsValidToken,
                UserId = component.User.Id
            };

            await _commandRepository.AddAsync(log);
        }
    }
}