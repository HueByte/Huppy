using Discord;
using Discord.WebSocket;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Huppy.Core.Services.EventService;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.ReminderService
{
    public class ReminderService : IReminderService
    {
        private readonly ILogger<ReminderService> _logger;
        private readonly IEventService _eventService;
        private readonly IReminderRepository _reminderRepository;
        private readonly DiscordShardedClient _discord;
        public ReminderService(IEventService eventService, ILogger<ReminderService> logger, DiscordShardedClient discord, IReminderRepository reminderRepository)
        {
            _eventService = eventService;
            _logger = logger;
            _discord = discord;
            _reminderRepository = reminderRepository;
        }

        public async Task Initialize()
        {
            var reminders = await _reminderRepository.GetAllAsync();
            await foreach (var reminder in reminders.ToAsyncEnumerable())
            {
                _eventService.AddEvent(reminder.RemindDate, reminder.Id.ToString(), async () => await StandardReminder(GetUser(reminder.UserId), reminder.Message));
            }
        }

        public async Task AddReminder(DateTime date, ulong userId, string message)
        {
            await AddReminder(date, GetUser(userId), message);
        }

        public async Task AddReminder(DateTime date, IUser user, string message)
        {
            Reminder reminder = new()
            {
                Message = message,
                RemindDate = date,
                UserId = user.Id
            };

            var resultId = await _reminderRepository.AddAsync(reminder);
            if (resultId is null) throw new Exception("Failed to create reminder");

            _eventService.AddEvent(date, resultId.ToString()!, async () => await StandardReminder(GetUser(reminder.UserId), reminder.Message));
        }

        public async Task RemoveReminder(int id)
        {
            var reminder = await _reminderRepository.GetAsync(id);
            await _reminderRepository.RemoveAsync(reminder!);
        }

        private IUser GetUser(ulong userId) => _discord.GetUser(userId);

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