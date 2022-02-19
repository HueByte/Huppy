using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IAiUsageRepository
    {
        Task AddAsync(AiUsage model);
    }
}