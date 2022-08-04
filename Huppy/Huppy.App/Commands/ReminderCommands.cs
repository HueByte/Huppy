using System.Net;
using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Huppy.Core.Services.ReminderService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    [Group("reminder", "reminder commands")]
    public class ReminderCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger<ReminderCommands> _logger;
        private readonly IReminderService _reminderService;
        public ReminderCommands(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        [SlashCommand("date", "Add reminder by date")]
        [Ephemeral]
        public async Task RemindMe(DateTime date, string message)
        {
            await _reminderService.AddReminder(date, Context.User, message);
            var embed = new EmbedBuilder().WithTitle("RemindMe result")
                                          .WithDescription($"Successfully created reminder at {TimestampTag.FromDateTime(date)}")
                                          .WithColor(Color.Teal)
                                          .Build();

            await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = embed;
            });
        }

        [SlashCommand("time", "Add reminder by time")]
        [Ephemeral]
        public async Task RemindMe(TimeSpan time, string message)
        {
            DateTime date = DateTime.UtcNow + time;
            await _reminderService.AddReminder(date, Context.User, message);

            var embed = new EmbedBuilder().WithTitle("RemindMe result")
                                          .WithDescription($"Successfully created reminder at {TimestampTag.FromDateTime(date)}")
                                          .WithColor(Color.Teal)
                                          .Build();

            await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = embed;
            });
        }
    }
}