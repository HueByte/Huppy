using System.Collections.ObjectModel;
using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Microsoft.Extensions.FileProviders;
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
            var messages = await Context.Channel.GetMessagesAsync(Context.Interaction.Id, Direction.Before, count).FlattenAsync();

            // check if message is ephemeral & date of the message is not 14 days away
            var filteredMessages = messages.Where(msg => msg.Flags != MessageFlags.Ephemeral && (DateTimeOffset.UtcNow - msg.Timestamp).TotalDays <= 14);

            _logger.LogInformation("Trying to remove {count} of messages", filteredMessages.Count());

            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(filteredMessages);

            var embed = new EmbedBuilder().WithTitle("Purge Report")
                .WithThumbnailUrl(Icons.Huppy1)
                .WithAuthor(Context.User)
                .WithColor(Color.LightOrange);

            if (count != filteredMessages.Count())
                embed.WithDescription("*Your input message and result message count might be different as bots cannot remove ephemeral messages*");

            var firstMessagesNumber = count > 10 ? 10 : count;

            var firstMessages = filteredMessages
                .Take(firstMessagesNumber)
                .Select(msg => new string($"> **{msg.Author}**# *{(!string.IsNullOrEmpty(msg.Content) ? msg.Content : "No Text")}*"));

            embed.AddField("Input number", $"`{count}`", true);
            embed.AddField("Messages removed", $"`{filteredMessages.Count()}`", true);
            embed.AddField("Date of removal", TimestampTag.FromDateTime(DateTime.UtcNow), true);

            if (firstMessages.Any())
                embed.AddField($"First {firstMessagesNumber} messages removed", string.Join("\n\n", firstMessages));

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}