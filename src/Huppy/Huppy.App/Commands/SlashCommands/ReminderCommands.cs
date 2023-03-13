using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.Paginator.Entities;
using Huppy.Core.Services.Reminder;
using Huppy.Core.Utilities;
using HuppyService.Service.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands.SlashCommands;

[Group("reminder", "reminder commands")]
public class ReminderCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly IReminderService _reminderService;
    private readonly IPaginatorService _paginatorService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private const int RemindersPerPage = 5;
    public ReminderCommands(IReminderService reminderService, IPaginatorService paginatorService, IServiceScopeFactory serviceScopeFactory)
    {
        _reminderService = reminderService;
        _paginatorService = paginatorService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    [SlashCommand("date", "Add reminder by date (format: dd/mm/yyyy hh:mm:ss)")]
    [Ephemeral]
    public async Task RemindMe(DateTime date, string message)
    {
        await AddRemindMe(date.ToUniversalTime(), message);
    }

    [SlashCommand("time", "Add reminder by time (format: 0d0h0m0s)")]
    [Ephemeral]
    public async Task RemindMe(TimeSpan time, string message)
    {
        DateTime date = DateTime.UtcNow + time;
        await AddRemindMe(date, message);
    }

    private async Task AddRemindMe(DateTime date, string message)
    {
        Embed embed;

        if (date > DateTime.UtcNow.AddYears(1))
        {
            embed = new EmbedBuilder()
            .WithTitle("Add reminder result")
                .WithDescription("Reminder cannot be more than 1 year ahead ")
                .WithColor(Color.Red)
                .Build();
        }
        else
        {
            await _reminderService.AddReminderAsync(date, Context.User, message);

            embed = new EmbedBuilder()
                .WithTitle("RemindMe result")
                .WithDescription($"Successfully created reminder at {TimestampTag.FromDateTime(date)}")
                .WithColor(Color.Teal)
                .Build();
        }

        await ModifyOriginalResponseAsync((msg) =>
        {
            msg.Embed = embed;
        });
    }

    [SlashCommand("remove", "remove your reminder")]
    [Ephemeral]
    public async Task RemoveReminder(int reminderId)
    {
        var reminder = await _reminderService.GetReminderAsync(Context.User.Id, reminderId);

        var embed = new EmbedBuilder().WithColor(Color.LightOrange)
            .WithCurrentTimestamp();

        if (reminder is null)
        {
            embed.WithTitle($"Couldn't remove reminder with {reminderId} ID")
                 .WithDescription("Ensure you provided proper reminder ID, you can check them via /reminder list");

            await ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = embed.Build();
            });

            return;
        }

        await _reminderService.RemoveReminderAsync(reminder);

        embed.WithTitle("Success")
             .WithDescription($"Reminder with `{reminderId}` ID got removed")
             .AddField("Date", TimestampTag.FromDateTime(DateTime.SpecifyKind(Miscellaneous.UnixTimeStampToUtcDateTime(reminder.RemindDate), DateTimeKind.Utc)))
             .AddField("Message", $"```vb\n{reminder.Message}\n```");

        await ModifyOriginalResponseAsync((msg) =>
        {
            msg.Embed = embed.Build();
        });
    }

    [SlashCommand("list", "Get list of your reminders")]
    [Ephemeral]
    public async Task GetUserReminders()
    {
        DynamicPaginatorEntry entry = new(_serviceScopeFactory)
        {
            MessageId = 0,
            CurrentPage = 0,
            Name = Guid.NewGuid().ToString(),
            Pages = new(),
            Data = Context.User
        };

        var remindersCount = await _reminderService.GetRemindersCount(Context.User.Id);

        if (!(remindersCount > 0))
        {
            var page = Task<PaginatorPage> (AsyncServiceScope scope, object? data) =>
            {
                if (data is not IUser user) return null!;

                var embed = new EmbedBuilder()
                    .WithTitle("Your reminders")
                    .WithAuthor(user)
                    .WithColor(Color.Orange)
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithDescription("You don't have any active reminders 🎈")
                    .WithCurrentTimestamp();

                var emptyPage = new PaginatorPage()
                {
                    Embed = embed,
                    Page = 0
                };

                return Task.FromResult(emptyPage);
            };

            entry.Pages.Add(page);
        }
        else
        {
            int pages = Convert.ToInt32(Math.Ceiling((decimal)remindersCount / RemindersPerPage));
            for (int i = 0; i < pages; i++)
            {
                int currentPage = i;
                var page = async Task<PaginatorPage> (AsyncServiceScope scope, object? data) =>
                {
                    if (data is not IUser user) return null!;

                    var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                    var reminders = await reminderService.GetSortedUserReminders(Context.User.Id, currentPage * RemindersPerPage, RemindersPerPage);

                    var embed = new EmbedBuilder()
                        .WithAuthor(user)
                        .WithThumbnailUrl(user.GetAvatarUrl())
                        .WithTitle("Your reminders")
                        .WithColor(Color.Orange)
                        .WithCurrentTimestamp();

                    foreach (var reminder in reminders)
                    {
                        TimestampTag timestamp = TimestampTag.FromDateTime(DateTime.SpecifyKind(Miscellaneous.UnixTimeStampToUtcDateTime(reminder.RemindDate), DateTimeKind.Utc));
                        embed.AddField(
                            $"✨ Reminder at {timestamp} | ID: {reminder.Id}",
                            $"```vb\n{reminder.Message}```",
                            false);
                    }

                    return new PaginatorPage()
                    {
                        Embed = embed,
                        Page = (ushort)currentPage
                    };
                };

                entry.Pages.Add(page);
            }
        }

        await _paginatorService.SendPaginatedMessage(Context.Interaction, entry);
    }
}
