using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Huppy.Core.Services.CommandService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    [Group("admin", "Moderation commands")]
    public class AdminCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
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
            if (count <= 0)
            {
                await ModifyOriginalResponseAsync((msg) =>
                {
                    msg.Embed = new EmbedBuilder()
                        .WithTitle("Purge Report")
                        .WithThumbnailUrl(Icons.Huppy1)
                        .WithAuthor(Context.User)
                        .WithColor(Color.LightOrange)
                        .WithDescription("Count number must be higher than 0")
                        .Build();
                });

                return;
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Interaction.Id, Direction.Before, count).FlattenAsync();

            // check if message is ephemeral & date of the message is not 14 days away
            var filteredMessages = messages.Where(msg => msg.Flags != MessageFlags.Ephemeral && (DateTimeOffset.UtcNow - msg.Timestamp).TotalDays <= 14);
            int filteredMessagesCount = filteredMessages.Count();

            _logger.LogInformation("Trying to remove {count} of messages", filteredMessagesCount);

            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(filteredMessages);

            var embed = new EmbedBuilder().WithTitle("Purge Report")
                .WithThumbnailUrl(Icons.Huppy1)
                .WithAuthor(Context.User)
                .WithColor(Color.LightOrange);

            if (count != filteredMessagesCount)
                embed.WithDescription("*Your input message and result message count might be different as bots cannot remove ephemeral messages*");

            var previewMessagesNumber = filteredMessagesCount > 10 ? 10 : filteredMessagesCount;

            var previewMessages = filteredMessages
                .Take(previewMessagesNumber)
                .Select(msg => new string($"> **{msg.Author}**# *{(!string.IsNullOrEmpty(msg.Content) ? msg.Content : "No Text")}*"));

            embed.AddField("Input number", $"`{count}`", true);
            embed.AddField("Messages removed", $"`{filteredMessagesCount}`", true);
            embed.AddField("Date of removal", TimestampTag.FromDateTime(DateTime.UtcNow), true);

            if (previewMessages.Any())
                embed.AddField($"First {(previewMessagesNumber > 1 ? $"`{previewMessagesNumber}` messages" : "one message")} removed", string.Join("\n\n", previewMessages));

            await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        }
    }
}