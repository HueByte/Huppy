using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class StatisticCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;

        public StatisticCommands(ILogger<StatisticCommands> logger)
        {
            _logger = logger;
        }

        [SlashCommand("GetStats", "Get statistics of the bot")]
        [RequireOwner]
        public async Task GetStatsAsync()
        {

        }
    }
}