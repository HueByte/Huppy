using Discord;
using Discord.Interactions;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.GPTService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class AiCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ICommandHandlerService _commandHandler;
        private readonly InteractionService _commands;
        private readonly ILogger _logger;
        private readonly IGPTService _aiService;

        public AiCommands(ICommandHandlerService commandHandler, InteractionService commands, ILogger<GeneralCommands> logger, IGPTService aiService)
        {
            _commandHandler = commandHandler;
            _commands = commands;
            _logger = logger;
            _aiService = aiService;
        }

        [SlashCommand("ai", "Talk with AI")]
        public async Task GptCompletion(string message)
        {
            var result = await _aiService.DavinciCompletion(message);

            var embed = new EmbedBuilder().WithCurrentTimestamp()
                                          .WithDescription(result)
                                          .WithTitle(message)
                                          .WithColor(Color.Teal)
                                          .WithAuthor(Context.User)
                                          .WithThumbnailUrl("https://i.pinimg.com/564x/69/2a/5b/692a5b4fcf71936d25ffdc01a62ca3a2.jpg");

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}