using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IReminderRepository
    {
        Task<IEnumerable<Reminder>> GetAllAsync();
        IQueryable<Reminder> GetQueryable(ulong userId);
        Task<Reminder?> GetAsync(int id);
        Task<int?> AddAsync(Reminder reminder);
        Task RemoveAsync(Reminder reminder);
        Task RemoveRangeAsync(IList<Reminder?> reminders);
        Task RemoveRangeAsync(ICollection<int> reminderIds);
    }
}