using Discord.Interactions;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class ReminderRepository : BaseRepository<int, Reminder, HuppyDbContext>, IReminderRepository
    {
        public ReminderRepository(HuppyDbContext context) : base(context) { }

        public async Task<Reminder?> GetAsync(ulong userId, int id)
        {
            return await _context.Reminders.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }

        public async Task RemoveRangeAsync(ICollection<int> reminderIds)
        {
            if (reminderIds is null) return;

            var reminders = await _context.Reminders
                .Where(reminder => reminderIds.Contains(reminder.Id))
                .ToListAsync();

            _context.Reminders.RemoveRange(reminders);
            await _context.SaveChangesAsync();
        }
    }
}