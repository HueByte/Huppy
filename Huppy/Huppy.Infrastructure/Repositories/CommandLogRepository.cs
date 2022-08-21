using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class CommandLogRepository : ICommandLogRepository, IRepository<int, CommandLog>
    {
        private readonly HuppyDbContext _context;
        public CommandLogRepository(HuppyDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetCount() => await _context.CommandLogs.CountAsync();

        public async Task<Dictionary<ulong, AiUser>> GetAiUsage()
        {
            Dictionary<ulong, AiUser> result = new();

            var commandLogs = await _context.CommandLogs?.Include(e => e.User)
                                                         .ToListAsync()!;

            var uniqueUsers = commandLogs.GroupBy(e => e.UserId)
                                         .Select(e => e.First())
                                         .ToList();

            foreach (var user in uniqueUsers)
            {
                result.TryAdd(user.UserId, new AiUser
                {
                    Username = user.User!.Username,
                    Count = commandLogs.Where(x => x.UserId == user.UserId)
                                       .Count()
                });
            }

            return result;
        }

        public async Task<CommandLog?> GetAsync(int id)
        {
            return await _context.CommandLogs.FirstOrDefaultAsync(commandLog => commandLog.Id == id);
        }

        public Task<IQueryable<CommandLog>> GetAllAsync()
        {
            return Task.FromResult(_context.CommandLogs.AsQueryable());
        }

        public async Task<bool> AddAsync(CommandLog? entity)
        {

            if (entity is null) return false;

            var doesExist = await _context.CommandLogs.AnyAsync(commandLog => commandLog.Id == entity.Id);
            if (doesExist) return false;

            await _context.CommandLogs.AddAsync(entity);
            return true;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<CommandLog> entities)
        {
            if (entities is null) return false;

            await _context.CommandLogs.AddRangeAsync(entities);
            return true;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            CommandLog commandLog = new() { Id = id };
            return await RemoveAsync(commandLog);
        }

        public async Task<bool> RemoveAsync(CommandLog? entity)
        {
            if (entity is null) return false;

            var doesExist = await _context.CommandLogs.AnyAsync(commandLog => commandLog.Id == entity.Id);
            if (!doesExist) return false;

            _context.CommandLogs.Remove(entity);
            return true;
        }

        public Task UpdateAsync(CommandLog? entity)
        {
            if (entity is null) return Task.CompletedTask;

            _context.CommandLogs.Update(entity);

            return Task.CompletedTask;
        }

        public async Task UpdateRange(IEnumerable<CommandLog> entities)
        {
            await _context.CommandLogs.AddRangeAsync(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}