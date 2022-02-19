using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IAiUsageRepository
    {
        Task AddAsync(AiUsage model);
        Task<List<AiUsage>> GetAll();
        Task<Dictionary<ulong, int>> GetUsage();
        Task<List<ulong>> GetUserIDs();
    }
}