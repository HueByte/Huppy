using Discord.Interactions;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly HuppyDbContext _context;
        public ReminderRepository(HuppyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reminder>> GetAllAsync()
        {
            return await _context.Reminders.ToListAsync();
        }

        public IQueryable<Reminder> GetQueryable()
        {
            return _context.Reminders.AsQueryable();
        }

        public IQueryable<Reminder> GetQueryable(ulong userId)
        {
            return _context.Reminders.Where(reminder => reminder.UserId == userId).AsQueryable();
        }

        public async Task<Reminder?> GetAsync(ulong userId, int id)
        {
            return await _context.Reminders.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }

        public async Task<int?> AddAsync(Reminder? reminder)
        {
            if (reminder is null) return null;

            var doesExist = await _context.Reminders.AnyAsync(e => e.Id == reminder.Id);
            if (doesExist) return null;

            await _context.Reminders.AddAsync(reminder);
            await _context.SaveChangesAsync();

            return reminder.Id;
        }

        public async Task AddRangeAsync(IList<Reminder?> reminders)
        {
            if (reminders is null) return;

            await _context.Reminders.AddRangeAsync(reminders!);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveByIdAsync(int id)
        {
            Reminder reminder = new() { Id = id };
            _context.Remove(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Reminder? reminder)
        {
            if (reminder is null) return;

            var doesExist = await _context.Reminders.AnyAsync(e => e.Id == reminder.Id);
            if (!doesExist) return;

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(IList<Reminder?> reminders)
        {
            if (reminders is null) return;

            _context.Reminders.RemoveRange(reminders!);
            await _context.SaveChangesAsync();
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