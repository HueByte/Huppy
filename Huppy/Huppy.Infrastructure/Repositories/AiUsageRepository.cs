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

        public async Task<List<AiUsage>> GetAll()
        {
            return await _context.AiUsages.ToListAsync();
        }

        public async Task<List<ulong>> GetUserIDs()
        {
            return await _context.AiUsages.Select(e => e.UserId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Dictionary<ulong, int>> GetUsage()
        {
            Dictionary<ulong, int> returnDictionary = new();

            var aiUsages = await _context.AiUsages.Where(x => x.Date!.Value.Month == DateTime.UtcNow.Month).ToListAsync();

            var uniqueUsersIds = aiUsages.Select(e => e.UserId).Distinct().ToList();

            foreach (var userId in uniqueUsersIds)
            {
                returnDictionary.TryAdd(userId, aiUsages.Where(e => e.UserId == userId).Count());
            }

            return returnDictionary;
        }

        public async Task GetStats()
        {
            var aiCommands = _context.AiUsages.ToList();
        }
    }
}