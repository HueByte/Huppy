using Discord;
using Huppy.Core.Models;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IReminderService
    {
        // Task Initialize();
        TimeSpan FetchPeriod { get; }
        Task RegisterFreshReminders();
        Task<List<Reminder>> GetUserRemindersAsync(ulong userId);
        Task AddReminder(DateTime date, ulong userId, string message);
        Task AddReminder(DateTime date, IUser user, string message);
        Task RemoveReminder(Reminder reminder);
        Task RemoveReminderRange(string[] ids);
        Task RemoveReminderRange(int[] ids);
    }
}