using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IReminderRepository
    {
        IEnumerable<Reminder> GetAllAsync();
        IQueryable<Reminder> GetAllQueryable();
        IQueryable<Reminder> GetAllQueryable(ulong userId);
        Task<Reminder?> GetAsync(ulong userId, int id);
        Task<int?> AddAsync(Reminder reminder);
        Task RemoveAsync(int id);
        Task RemoveAsync(Reminder reminder);
        Task RemoveRangeAsync(IList<Reminder?> reminders);
        Task RemoveRangeAsync(ICollection<int> reminderIds);
    }
}