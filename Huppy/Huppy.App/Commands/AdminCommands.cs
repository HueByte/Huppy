using System.Collections.ObjectModel;
using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    [Group("admin", "Moderation commands")]
    public class AdminCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        public AdminCommands(ILogger<AdminCommands> logger)
        {
            _logger = logger;
        }

        [SlashCommand("purge", "Removes specified number of messages to maximum of 14 days prior current date")]
        [Ephemeral]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [EnabledInDm(true)]
        public async Task PurgeMessages(int count)
        {
            var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();

            _logger.LogInformation("Trying to remove {count} of messages", messages.Count());

            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);

            var embed = new EmbedBuilder().WithTitle("Purge result")
                .WithThumbnailUrl(Icons.Huppy1)
                .WithAuthor(Context.User)
                .WithColor(Color.LightOrange);

            embed.AddField("Input number", $"`{count}`", true);
            embed.AddField("Messages removed", $"`{messages.Count()}`", true);
            embed.AddField("Date of removal", TimestampTag.FromDateTime(DateTime.UtcNow), true);

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}