using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.Common.Constants;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.AiStabilizerService;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class StatisticCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly CacheService _cacheService;
        public StatisticCommands(ILogger<StatisticCommands> logger, CacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
        }

        [SlashCommand("aistats", "Get statistics of the bot")]
        public async Task GetAiStats()
        {
            var stats = await _cacheService.GetAiStatistics();
            var topStats = stats.OrderByDescending(x => x.Value.Count).Take(5);

            StringBuilder sb = new();
            sb.AppendLine("✨ Top Huppy friends ✨\n");
            foreach (var item in topStats)
            {
                sb.AppendLine($"> **{item.Value.Username}** : `{item.Value.Count}`\n");
            }

            var embed = new EmbedBuilder().WithCurrentTimestamp()
                                          .WithTitle("Statistics for AI service usage")
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithColor(Color.Magenta)
                                          .WithDescription(sb.ToString());

            embed.AddField("Total commands used", stats.Sum(x => x.Value.Count), true);
            embed.AddField("Huppy friend count", stats.Keys.Count, true);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}