using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Utilities;
using Huppy.Kernel.Constants;
using HuppyService.Service.Protos;
using HuppyService.Service.Protos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.Reminder;

// public record ReminderInput(IUser User, string Message);

public class ReminderService : IReminderService
{
    private readonly ILogger _logger;
    private readonly IEventLoopService _eventService;
    private readonly InteractionService _interactionService;
    private DateTime FetchingDate => DateTime.UtcNow + FetchReminderFrequency;
    public TimeSpan FetchReminderFrequency { get; } = new(1, 0, 0);
    private readonly ReminderProto.ReminderProtoClient _reminderClient;
    public ReminderService(IEventLoopService eventService, ILogger<ReminderService> logger, DiscordShardedClient discord, InteractionService interactionService, ITimedEventsService timedEventsService, ReminderProto.ReminderProtoClient reminderClient)
    {
        _eventService = eventService;
        _logger = logger;
        _interactionService = interactionService;
        _reminderClient = reminderClient;
    }

    public async Task<IList<ReminderModel>> GetSortedUserReminders(ulong userId, int skip, int take)
    {
        var result = await _reminderClient.GetSortedUserRemindersAsync(new() { UserId = userId, Skip = skip, Take = take });
        return result.ReminderModels;
    }

    public async Task<int> GetRemindersCount(ulong userId)
    {
        var result = await _reminderClient.GetRemindersCountAsync(new GetReminderCountInput() { UserId = userId });
        return result.Count;
    }

    public async Task<ReminderModel> GetReminderAsync(ulong userId, int reminderId)
    {
        return await _reminderClient.GetReminderAsync(new GetReminderInput() { ReminderId = reminderId, UserId = userId });
    }

    public async Task RegisterFreshRemindersAsync()
    {
        var reminders = await _reminderClient.GetReminderBatchAsync(new ReminderBatchInput()
        {
            StartDate = Miscellaneous.DateTimeToUnixTimestamp(DateTime.UtcNow),
            EndDate = Miscellaneous.DateTimeToUnixTimestamp(DateTime.UtcNow + FetchReminderFrequency)
        });

        _logger.LogInformation("Registering fresh bulk of reminders");

        if (!reminders.ReminderModels.Any()) return;

        // start adding reminder in async parallel manner
        var jobs = reminders.ReminderModels.Select(reminder => Task.Run(async () =>
        {
            // fetch user
            var user = await GetUser(reminder.UserId);

            // add reminder
            ReminderInput reminderInput = new() { User = user, Message = reminder.Message };
            await _eventService.AddEvent(Miscellaneous.UnixTimeStampToDateTime(reminder.UnixTime), reminder.Id.ToString(), reminderInput, async (input) =>
            {
                if (input is ReminderInput data)
                {
                    await StandardReminder(data.User, data.Message!);
                }
            });
        })).ToList();

        await Task.WhenAll(jobs);

        _logger.LogInformation("Enqueued {count} of reminders to execute until {time}", reminders.ReminderModels.Count, FetchingDate);
    }

    public async Task<IList<ReminderModel>> GetUserRemindersAsync(ulong userId)
    {
        return (await _reminderClient.GetUserRemindersAsync(new ReminderUserInput() { UserId = userId })).ReminderModels;
    }

    public async Task AddReminderAsync(DateTime date, ulong userId, string message)
    {
        await AddReminderAsync(date, await GetUser(userId), message);
    }

    public async Task AddReminderAsync(DateTime date, IUser user, string message)
    {
        date = date.ToUniversalTime();
        ReminderModel reminder = new()
        {
            Message = message,
            UnixTime = Miscellaneous.DateTimeToUnixTimestamp(date),
            UserId = user.Id
        };

        reminder = await _reminderClient.AddReminderAsync(reminder);

        if (reminder.Id == 0) throw new Exception("Failed to create reminder");

        ReminderInput reminderInput = new() { User = user, Message = reminder.Message };

        _logger.LogInformation("Added reminder for [{user}] at [{date}] UTC", user.Username, date);

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

    public async Task RemoveReminderAsync(ReminderModel reminder)
    {
        var result = await _reminderClient.RemoveReminderAsync(reminder);

        if (result.IsSuccess) throw new Exception($"Failed to remove reminder {reminder.Id}");

        await _eventService.Remove(Miscellaneous.UnixTimeStampToDateTime(reminder.UnixTime), reminder.Id.ToString());
    }

    public async Task RemoveReminderRangeAsync(string[] ids)
    {
        if (ids.Length <= 0) return;

        int[] castedIds = ids.Select(int.Parse).ToArray();
        await RemoveReminderRangeAsync(castedIds);
    }

    public async Task RemoveReminderRangeAsync(int[] ids)
    {
        RemoveReminderRangeInput request = new();
        request.Ids.AddRange(ids);

        await _reminderClient.RemoveReminderRangeAsync(request);
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
