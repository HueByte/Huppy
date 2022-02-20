using System.Linq;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class AiUsageRepository : IAiUsageRepository
    {
        private readonly HuppyDbContext _context;
        public AiUsageRepository(HuppyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AiUsage model)
        {
            await _context.AddAsync(model);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ulong>> GetUserIDs()
        {
            return await _context.AiUsages.Select(e => e.UserId)
                .Distinct()
                .ToListAsync();
        }
        public async Task<Dictionary<ulong, AiUser>> GetUsage()
        {
            Dictionary<ulong, AiUser> returnDictionary = new();

            var usages = await _context.AiUsages.Where(x => x.Date!.Value.Month == DateTime.UtcNow.Month).ToListAsync();

            var uniqueUsers = usages.GroupBy(e => e.UserId).Select(e => e.First()).ToList();

            foreach (var user in uniqueUsers)
            {
                returnDictionary.TryAdd(user.UserId, new AiUser
                {
                    Username = user.Username,
                    Count = usages.Where(x => x.UserId == user.UserId).Count()
                });
            }

            return returnDictionary;
        }
    }
}