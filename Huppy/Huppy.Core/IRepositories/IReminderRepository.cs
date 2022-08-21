using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IReminderRepository : IRepository<int, Reminder>
    {
        Task<Reminder?> GetAsync(ulong userId, int id);
        Task RemoveRangeAsync(ICollection<int> reminderIds);
    }
}