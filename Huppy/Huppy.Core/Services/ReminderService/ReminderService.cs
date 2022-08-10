using System.Net;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Huppy.Core.Services.EventService;
using Huppy.Core.Services.TimedEventsService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.ReminderService
{
    public record ReminderInput(IUser User, string Message);
    public class ReminderService : IReminderService
    {
        private readonly ILogger<ReminderService> _logger;
        private readonly IEventService _eventService;
        private readonly IReminderRepository _reminderRepository;
        private readonly DiscordShardedClient _discord;
        private readonly InteractionService _interactionService;
        private readonly ITimedEventsService _timedEventsService;
        private readonly TimeSpan fetchPeriod = new(0, 1, 0);
        private DateTime FetchingDate => DateTime.UtcNow + fetchPeriod;
        public ReminderService(IEventService eventService, ILogger<ReminderService> logger, DiscordShardedClient discord, IReminderRepository reminderRepository, InteractionService interactionService, ITimedEventsService timedEventsService)
        {
            _eventService = eventService;
            _logger = logger;
            _discord = discord;
            _reminderRepository = reminderRepository;
            _interactionService = interactionService;
            _timedEventsService = timedEventsService;
        }

        // TODO
        // guild.DownloadUsersAsync, and within DiscordSocketConfig there's alwaysdownloadusers 
        // Will use bulk download method 
        public Task Initialize()
        {
            // every {fetchingPeriod} 
            _logger.LogInformation("Starting Reminder Service");
            _timedEventsService.AddJob(
                Guid.NewGuid(),
                null,
                new TimeSpan(0),
                fetchPeriod,
                async (scope, data) =>
                {
                    var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                    await reminderService.RegisterFreshReminders();
                }
            );

            return Task.CompletedTask;
        }

        public async Task RegisterFreshReminders()
        {
            _logger.LogInformation("Registering fresh bulk of reminders");

            // fetch reminders before fetchPeriod date
            var reminders = await _reminderRepository
                .GetQueryable()
                .Where(reminder => reminder.RemindDate < FetchingDate)
                .ToListAsync();

            if (!reminders.Any()) return;

            // start adding reminder in async parallel manner
            var jobs = reminders.Select(reminder => Task.Run(async () =>
            {
                // fetch user
                var user = await GetUser(reminder.UserId);

                // add reminder
                ReminderInput reminderInput = new(user, reminder.Message);
                await _eventService.AddEvent(reminder.RemindDate, reminder.Id.ToString(), reminderInput, async (input) =>
                {
                    if (input is ReminderInput data)
                    {
                        await StandardReminder(data.User, data.Message);
                    }
                });
            })).ToList();

            await Task.WhenAll(jobs);
            _logger.LogInformation("Enqueued {count} of reminders to execute until {time}", reminders.Count, FetchingDate);
        }

        public async Task<List<Reminder>> GetUserRemindersAsync(ulong userId)
        {
            return await _reminderRepository.GetQueryable(userId).ToListAsync();
        }

        public async Task AddReminder(DateTime date, ulong userId, string message)
        {
            await AddReminder(date, await GetUser(userId), message);
        }

        public async Task AddReminder(DateTime date, IUser user, string message)
        {
            date = date.ToUniversalTime();
            Reminder reminder = new()
            {
                Message = message,
                RemindDate = date,
                UserId = user.Id
            };

            var resultId = await _reminderRepository.AddAsync(reminder);
            if (resultId is null) throw new Exception("Failed to create reminder");

            ReminderInput reminderInput = new(user, message);
            _logger.LogInformation("Added reminder for [{user}] at [{date}] UTC", user.Username, reminder.RemindDate);

            if (date < FetchingDate)
            {
                await _eventService.AddEvent(date, resultId.ToString()!, reminderInput, async (input) =>
                {
                    if (input is ReminderInput data)
                    {
                        await StandardReminder(data.User, data.Message);
                    }
                });
            }
        }

        public async Task RemoveReminder(int id)
        {
            var reminder = await _reminderRepository.GetAsync(id);
            await _reminderRepository.RemoveAsync(reminder!);
        }

        public async Task RemoveReminderRange(string[] ids)
        {
            int[] castedIds = ids.Select(int.Parse).ToArray();
            await RemoveReminderRange(castedIds);
        }

        public async Task RemoveReminderRange(int[] ids)
        {
            if (!(ids.Length > 0)) return;

            await _reminderRepository.RemoveRangeAsync(ids);
        }

        private async Task StandardReminder(IUser user, string message)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Your reminder")
                .WithColor(Color.Teal)
                .WithThumbnailUrl(Common.Constants.Icons.Huppy1)
                .WithDescription(message)
                .WithCurrentTimestamp()
                .Build();

            await user.SendMessageAsync("", false, embed);

            _logger.LogInformation("Sent reminder to [{user}]", user.Username);
        }

        private async Task<IUser> GetUser(ulong userId) => await _interactionService.RestClient.GetUserAsync(userId);
    }
}