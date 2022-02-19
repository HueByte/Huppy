using Discord;
using Discord.Interactions;
using Huppy.Core.Services.AiStabilizerService;
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
        private readonly IAiStabilizerService _stabilizerService;

        public AiCommands(ICommandHandlerService commandHandler, InteractionService commands, ILogger<GeneralCommands> logger, IGPTService aiService, IAiStabilizerService stabilizerService)
        {
            _commandHandler = commandHandler;
            _commands = commands;
            _logger = logger;
            _aiService = aiService;
            _stabilizerService = stabilizerService;
        }

        [SlashCommand("ai", "Talk with Huppy!")]
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
            await _stabilizerService.LogUsageAsync(message, Context.User.Username, Context.User.Id, result.Trim());
        }
    }
}