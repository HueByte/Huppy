using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Kernel.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.Reminder;

// public record ReminderInput(IUser User, string Message);

public class ReminderService : IReminderService
{
    private readonly ILogger _logger;
    private readonly IEventLoopService _eventService;
    private readonly IReminderRepository _reminderRepository;
    private readonly InteractionService _interactionService;
    private DateTime FetchingDate => DateTime.UtcNow + FetchPeriod;
    public TimeSpan FetchPeriod { get; } = new(1, 0, 0);
    public ReminderService(IEventLoopService eventService, ILogger<ReminderService> logger, DiscordShardedClient discord, IReminderRepository reminderRepository, InteractionService interactionService, ITimedEventsService timedEventsService)
    {
        _eventService = eventService;
        _logger = logger;
        _reminderRepository = reminderRepository;
        _interactionService = interactionService;
    }

    public async Task RegisterFreshReminders()
    {
        _logger.LogInformation("Registering fresh bulk of reminders");

        // fetch reminders before fetchPeriod date
        var remindersQueryable = await _reminderRepository.GetAllAsync();

        var reminders = await remindersQueryable
            .Where(reminder => reminder.RemindDate < FetchingDate)
            .ToListAsync();

        if (!reminders.Any()) return;

        // start adding reminder in async parallel manner
        var jobs = reminders.Select(reminder => Task.Run(async () =>
        {
            // fetch user
            var user = await GetUser(reminder.UserId);

            // add reminder
            ReminderInput reminderInput = new() { User = user, Message = reminder.Message };
            await _eventService.AddEvent(reminder.RemindDate, reminder.Id.ToString(), reminderInput, async (input) =>
            {
                if (input is ReminderInput data)
                {
                    await StandardReminder(data.User, data.Message!);
                }
            });
        })).ToList();

        await Task.WhenAll(jobs);

        _logger.LogInformation("Enqueued {count} of reminders to execute until {time}", reminders.Count, FetchingDate);
    }

    public async Task<List<Models.Reminder>> GetUserRemindersAsync(ulong userId)
    {
        var reminders = await _reminderRepository.GetAllAsync();

        return await reminders.Where(reminder => reminder.UserId == userId)
                              .ToListAsync();
    }

    public async Task AddReminder(DateTime date, ulong userId, string message)
    {
        await AddReminder(date, await GetUser(userId), message);
    }

    public async Task AddReminder(DateTime date, IUser user, string message)
    {
        date = date.ToUniversalTime();
        Models.Reminder reminder = new()
        {
            Message = message,
            RemindDate = date,
            UserId = user.Id
        };

        var result = await _reminderRepository.AddAsync(reminder);
        await _reminderRepository.SaveChangesAsync();

        if (!result) throw new Exception("Failed to create reminder");

        ReminderInput reminderInput = new() { User = user, Message = reminder.Message };

        _logger.LogInformation("Added reminder for [{user}] at [{date}] UTC", user.Username, reminder.RemindDate);

        if (date < FetchingDate)
        {
            await _eventService.AddEvent(date, reminder.Id.ToString()!, reminderInput, async (input) =>
            {
                if (input is ReminderInput data)
                {
                    data.Message ??= "";
                    await StandardReminder(data.User, data.Message);
                }
            });
        }
    }

    public async Task RemoveReminder(Models.Reminder reminder)
    {
        await _reminderRepository.RemoveAsync(reminder);
        await _reminderRepository.SaveChangesAsync();
        await _eventService.Remove(reminder.RemindDate, reminder.Id.ToString());
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
        await _reminderRepository.SaveChangesAsync();
    }

    private async Task StandardReminder(IUser user, string message)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Your reminder")
            .WithColor(Color.Teal)
            .WithThumbnailUrl(Icons.Huppy1)
            .WithDescription(message)
            .WithCurrentTimestamp()
            .Build();

        await user.SendMessageAsync("", false, embed);

        _logger.LogInformation("Sent reminder to [{user}]", user.Username);
    }

    private async Task<IUser> GetUser(ulong userId) => await _interactionService.RestClient.GetUserAsync(userId);
}
