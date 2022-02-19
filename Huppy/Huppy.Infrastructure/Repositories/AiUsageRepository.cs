using Huppy.Core.IRepositories;
using Huppy.Core.Models;

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

        public async Task GetStats()
        {
            var aiCommands = _context.AiUsages.ToList();
        }
    }
}