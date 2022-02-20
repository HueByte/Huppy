using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.AiStabilizerService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class StatisticCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly IAiStabilizerService _stabilizerService;
        private readonly IAiUsageRepository _usageRepository;

        public StatisticCommands(ILogger<StatisticCommands> logger, IAiStabilizerService stabilizerService, IAiUsageRepository usageRepository)
        {
            _logger = logger;
            _stabilizerService = stabilizerService;
            _usageRepository = usageRepository;
        }

        [SlashCommand("aistats", "Get statistics of the bot")]
        [RequireOwner]
        public async Task GetAiStats()
        {
            var stats = await _stabilizerService.GetStatistics();
            var topStats = stats.OrderByDescending(x => x.Value.Count).Take(5);

            StringBuilder sb = new();
            sb.AppendLine("✨ Top Huppy friends ✨\n");
            foreach (var item in topStats)
            {
                sb.AppendLine($"> **{item.Value.Username}** : `{item.Value.Count}`\n");
            }

            var embed = new EmbedBuilder().WithCurrentTimestamp()
                                          .WithTitle("Statistics for AI service usage")
                                          .WithThumbnailUrl("https://i.pinimg.com/564x/69/2a/5b/692a5b4fcf71936d25ffdc01a62ca3a2.jpg")
                                          .WithColor(Color.Magenta)
                                          .WithDescription(sb.ToString());

            embed.AddField("Total commands used: ", stats.Sum(x => x.Value.Count));

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}