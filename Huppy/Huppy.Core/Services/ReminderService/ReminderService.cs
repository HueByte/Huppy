using Discord;
using Huppy.Core.Services.EventService;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.ReminderService
{
    public class ReminderService : IReminderService
    {
        private readonly ILogger<ReminderService> _logger;
        private readonly IEventService _eventService;
        public ReminderService(IEventService eventService, ILogger<ReminderService> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        public void AddReminder(DateTime date, IUser user, string message)
        {
            _eventService.AddEvent(date, "ReminderEvent", async () => await StandardReminder(user, message));
        }

        private async Task StandardReminder(IUser user, string message)
        {
            var embed = new EmbedBuilder().WithTitle("Your reminder")
                .WithColor(Color.Teal)
                .WithThumbnailUrl(Common.Constants.Icons.Huppy1)
                .WithDescription(message)
                .WithCurrentTimestamp()
                .Build();

            await user.SendMessageAsync("", false, embed);
            _logger.LogInformation("Sent reminder to [{user}]", user.Username);
        }
    }
}