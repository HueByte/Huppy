using HuppyService.Core.Abstraction;
using HuppyService.Core.Models;

namespace HuppyService.Core.Interfaces.IRepositories
{
    public interface IReminderRepository : IRepository<int, Reminder>
    {
        Task<Reminder?> GetAsync(ulong userId, int id);
        Task RemoveRangeAsync(ICollection<int> reminderIds);
    }
}