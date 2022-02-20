using System.Runtime.CompilerServices;
using Discord;
using Discord.Interactions;
using Huppy.Core.Common.Constants;
using Huppy.Core.Entities;
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
        private readonly AppSettings _appSettings;

        public AiCommands(ICommandHandlerService commandHandler, InteractionService commands, ILogger<GeneralCommands> logger, IGPTService aiService, IAiStabilizerService stabilizerService, AppSettings appSettings)
        {
            _commandHandler = commandHandler;
            _commands = commands;
            _logger = logger;
            _aiService = aiService;
            _stabilizerService = stabilizerService;
            _appSettings = appSettings;
        }

        [SlashCommand("ai", "Talk with Huppy!")]
        public async Task GptCompletion(string message)
        {
            // var messageCount = await _stabilizerService.GetCurrentMessageCount();
            // if (messageCount > _appSettings.GPT!.FreeMessageQuota)
            // {
            //     await ModifyOriginalResponseAsync((msg) => msg.Content = "There's no free quota available now");
            //     return;
            // }

            var result = await _aiService.DavinciCompletion(message);

            var embed = new EmbedBuilder().WithCurrentTimestamp()
                                          .WithDescription(result)
                                          .WithTitle(message)
                                          .WithColor(Color.Teal)
                                          .WithAuthor(Context.User)
                                          .WithThumbnailUrl(Icons.Huppy1);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
            await _stabilizerService.LogUsageAsync(Context.User.Username, Context.User.Id);
        }
    }
}