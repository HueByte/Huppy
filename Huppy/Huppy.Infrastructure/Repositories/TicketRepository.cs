using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository, IRepository<string, Ticket>
    {
        private readonly HuppyDbContext _context;
        public TicketRepository(HuppyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Ticket? entity)
        {
            if (entity is null) return false;

            var doesExist = await _context.Tickets.AnyAsync(ticked => ticked.Id == entity.Id);
            if (doesExist) return false;

            await _context.Tickets.AddAsync(entity);
            return true;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<Ticket> entities)
        {
            if (entities is null) return false;

            await _context.Tickets.AddRangeAsync(entities);
            return true;
        }

        public Task<IQueryable<Ticket>> GetAllAsync()
        {
            return Task.FromResult(_context.Tickets.AsQueryable());
        }

        public async Task<Ticket?> GetAsync(string id)
        {
            return await _context.Tickets.FirstOrDefaultAsync(ticked => ticked.Id == id);
        }

        public async Task<bool> RemoveAsync(string id)
        {
            Ticket ticket = new() { Id = id };
            return await RemoveAsync(ticket);
        }

        public async Task<bool> RemoveAsync(Ticket? entity)
        {
            if (entity is null) return false;

            var doesExist = await _context.Tickets.AnyAsync(ticket => ticket.Id == entity.Id);
            if (!doesExist) return false;

            _context.Tickets.Remove(entity);
            return true;
        }

        public Task UpdateAsync(Ticket? entity)
        {
            if (entity is null) return Task.CompletedTask;

            _context.Tickets.Update(entity);

            return Task.CompletedTask;
        }

        public async Task UpdateRange(IEnumerable<Ticket> entities)
        {
            await _context.Tickets.AddRangeAsync(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}