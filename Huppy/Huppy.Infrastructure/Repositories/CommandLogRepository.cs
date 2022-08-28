using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class CommandLogRepository : BaseRepository<int, CommandLog, HuppyDbContext>, ICommandLogRepository
    {
        public CommandLogRepository(HuppyDbContext context) : base(context) { }

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
    }
}