using System.Runtime.CompilerServices;
using Discord;
using Discord.Interactions;
using Huppy.Core.Common.Constants;
using Huppy.Core.Entities;
using Huppy.Core.Services.AiStabilizerService;
using Huppy.Core.Services.CommandService;
using Huppy.Core.Services.GPTService;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class AiCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly IGPTService _aiService;
        private readonly CacheService _cacheService;
        public AiCommands(IGPTService aiService, CacheService cacheService)
        {
            _aiService = aiService;
            _cacheService = cacheService;
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
            await _cacheService.LogUsageAsync(Context.User.Username, Context.User.Id);
        }
    }
}