using Discord;

namespace Huppy.Core.Services.ReminderService
{
    public interface IReminderService
    {
        Task Initialize();
        Task AddReminder(DateTime date, ulong userId, string message);
        Task AddReminder(DateTime date, IUser user, string message);
    }
}