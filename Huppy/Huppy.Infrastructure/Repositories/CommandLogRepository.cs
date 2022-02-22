using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class CommandLogRepository : ICommandLogRepository
    {
        private readonly HuppyDbContext _context;
        public CommandLogRepository(HuppyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CommandLog commandLog)
        {
            await _context.CommandLogs.AddAsync(commandLog);
            await _context.SaveChangesAsync();
        }

        public async Task AddRange(CommandLog[] commandLogs)
        {
            await _context.CommandLogs.AddRangeAsync(commandLogs);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CommandLog>> GetAll()
        {
            return await _context.CommandLogs.ToListAsync();
        }

        public Task<int> GetCount()
        {
            return Task.FromResult(_context.CommandLogs.Count());
        }

        public async Task<Dictionary<ulong, AiUser>> GetAiUsage()
        {
            Dictionary<ulong, AiUser> result = new();

            var commandLogs = await _context.CommandLogs?.Include(e => e.User)
                                                         .Where(x => x.Date!.Value.Month == DateTime.UtcNow.Month && x.CommandName == "chat")
                                                         .ToListAsync()!;

            var uniqueUsers = commandLogs.GroupBy(e => e.UserId)
                                         .Select(e => e.First())
                                         .ToList();

            foreach (var user in uniqueUsers)
            {
                result.TryAdd(user.UserId, new AiUser
                {
                    Username = user.User!.Username,
                    Count = commandLogs.Where(x => x.UserId == user.UserId).Count()
                });
            }

            return result;
        }
    }
}