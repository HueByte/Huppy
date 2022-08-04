using Discord;

namespace Huppy.Core.Services.ReminderService
{
    public interface IReminderService
    {
        void AddReminder(DateTime date, IUser user, string message);
    }
}