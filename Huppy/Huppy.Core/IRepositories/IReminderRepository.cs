using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IReminderRepository
    {
        Task<IEnumerable<Reminder>> GetAllAsync();
        IQueryable<Reminder> GetRange(ulong userId);
        Task<Reminder?> GetAsync(int id);
        Task<int?> AddAsync(Reminder reminder);
        Task RemoveAsync(Reminder reminder);
        Task RemoveRange(IList<Reminder?> reminders);
        Task RemoveRange(ICollection<int> reminderIds);
    }
}