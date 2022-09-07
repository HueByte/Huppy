using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.Paginator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands.SlashCommands;

[Group("reminder", "reminder commands")]
public class ReminderCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly ILogger<ReminderCommands> _logger;
    private readonly IReminderService _reminderService;
    private readonly IPaginatorService _paginatorService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IReminderRepository _reminderRepository;
    private const int RemindersPerPage = 5;
    public ReminderCommands(IReminderService reminderService, IPaginatorService paginatorService, IServiceScopeFactory serviceScopeFactory, IReminderRepository reminderRepository)
    {
        _reminderService = reminderService;
        _paginatorService = paginatorService;
        _serviceScopeFactory = serviceScopeFactory;
        _reminderRepository = reminderRepository;
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
            await _reminderService.AddReminder(date, Context.User, message);

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
        var reminder = await _reminderRepository.GetAsync(Context.User.Id, reminderId);

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

        await _reminderService.RemoveReminder(reminder);

        embed.WithTitle("Success")
             .WithDescription($"Reminder with `{reminderId}` ID got removed")
             .AddField("Date", TimestampTag.FromDateTime(DateTime.SpecifyKind(reminder.RemindDate, DateTimeKind.Utc)))
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

        var reminders = await _reminderRepository.GetAllAsync();
        var reminderCount = await reminders.Where(entry => entry.UserId == Context.User.Id).CountAsync();

        if (!(reminderCount > 0))
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
            int pages = Convert.ToInt32(Math.Ceiling((decimal)reminderCount / RemindersPerPage));
            for (int i = 0; i < pages; i++)
            {
                int currentPage = i;
                var page = async Task<PaginatorPage> (AsyncServiceScope scope, object? data) =>
                {
                    if (data is not IUser user) return null!;

                    var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();

                    var reminders = await reminderRepository.GetAllAsync();
                    var pageReminders = await reminders
                        .Where(reminder => reminder.UserId == user.Id)
                        .OrderBy(e => e.RemindDate)
                        .Skip(currentPage * RemindersPerPage)
                        .Take(RemindersPerPage)
                        .ToListAsync();

                    var embed = new EmbedBuilder()
                        .WithAuthor(user)
                        .WithThumbnailUrl(user.GetAvatarUrl())
                        .WithTitle("Your reminders")
                        .WithColor(Color.Orange)
                        .WithCurrentTimestamp();

                    foreach (var reminder in pageReminders)
                    {
                        TimestampTag timestamp = TimestampTag.FromDateTime(DateTime.SpecifyKind(reminder.RemindDate, DateTimeKind.Utc));
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