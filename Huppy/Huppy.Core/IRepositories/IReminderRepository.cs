using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IReminderRepository
    {
        Task<IEnumerable<Reminder>> GetAllAsync();
        Task<Reminder?> GetAsync(int id);
        Task<int?> AddAsync(Reminder reminder);
        Task RemoveAsync(Reminder reminder);
        Task RemoveRange(IList<Reminder?> reminders);
    }
}