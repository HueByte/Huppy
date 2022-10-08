using Huppy.Core.Models;
using Huppy.Kernel;

namespace Huppy.Core.Interfaces.IRepositories
{
    [Obsolete]
    public interface IReminderRepository : IRepository<int, Reminder>
    {
        Task<Reminder?> GetAsync(ulong userId, int id);
        Task RemoveRangeAsync(ICollection<int> reminderIds);
    }
}