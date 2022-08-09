using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Huppy.Core.IRepositories;
using Huppy.Core.Services.PaginatorService;
using Huppy.Core.Services.PaginatorService.Entities;
using Huppy.Core.Services.ReminderService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    [Group("reminder", "reminder commands")]
    public class ReminderCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger<ReminderCommands> _logger;
        private readonly IReminderService _reminderService;
        private readonly IPaginatorService _paginatorService;
        IServiceScopeFactory _serviceScopeFactory;
        private const int RemindersPerPage = 6;
        public ReminderCommands(IReminderService reminderService, IPaginatorService paginatorService, IServiceScopeFactory serviceScopeFactory)
        {
            _reminderService = reminderService;
            _paginatorService = paginatorService;
            _serviceScopeFactory = serviceScopeFactory;
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

        // [SlashCommand("list", "Get list of your reminders")]
        // [Ephemeral]
        public async Task GetUserReminders()
        {
            DynamicPaginatorEntry entry = new(_serviceScopeFactory)
            {
                MessageId = 0,
                CurrentPage = 0,
                Name = "User reminders",
                Pages = new()
            };
            List<Func<AsyncServiceScope, Task<PaginatorPage>>> PageHandlerSelectedContext = new();

            var reminders = await _reminderService.GetUserRemindersAsync(Context.User.Id);
            for (int i = 0; i < reminders.Count * RemindersPerPage; i++)
            {
                Func<AsyncServiceScope, Task<PaginatorPage>> page = async (scope) =>
                {
                    var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
                    // var pageReminders = reminderRepository.GetRange()
                    var embed = new EmbedBuilder();

                    return new PaginatorPage()
                    {
                        Embed = embed,
                        Page = (ushort)i
                    };
                };
            }
        }
    }
}