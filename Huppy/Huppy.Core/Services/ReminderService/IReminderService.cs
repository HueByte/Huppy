using Discord;
using Huppy.Core.Models;

namespace Huppy.Core.Services.ReminderService
{
    public interface IReminderService
    {
        Task Initialize();
        Task RegisterFreshReminders();
        Task<List<Reminder>> GetUserRemindersAsync(ulong userId);
        Task AddReminder(DateTime date, ulong userId, string message);
        Task AddReminder(DateTime date, IUser user, string message);
        Task RemoveReminder(int id);
        Task RemoveReminderRange(string[] ids);
        Task RemoveReminderRange(int[] ids);
    }
}