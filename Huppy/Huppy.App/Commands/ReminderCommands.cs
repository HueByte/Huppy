using System.Collections.ObjectModel;
using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.PaginatorService;
using Huppy.Core.Services.PaginatorService.Entities;
using Huppy.Core.Services.ReminderService;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Huppy.App.Commands
{
    [Group("reminder", "reminder commands")]
    public class ReminderCommands : InteractionModuleBase<ShardedInteractionContext>
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

        [SlashCommand("date", "Add reminder by date (format: dd/mm/yyyy 00:00:00)")]
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
                Data = (object?)Context.User
            };

            var reminderCount = await _reminderRepository.GetQueryable(Context.User.Id).CountAsync();

            if (!(reminderCount > 0))
            {
                Func<AsyncServiceScope, object?, Task<PaginatorPage>> page = (scope, data) =>
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
                    Func<AsyncServiceScope, object?, Task<PaginatorPage>> page = async (scope, data) =>
                    {
                        if (data is not IUser user) return null!;

                        var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();

                        var pageReminders = await reminderRepository
                            .GetQueryable(user.Id)
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
                                $"✨ Reminder at {timestamp}",
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
}