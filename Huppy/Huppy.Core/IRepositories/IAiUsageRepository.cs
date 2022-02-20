using Huppy.Core.Entities;
using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IAiUsageRepository
    {
        Task AddAsync(AiUsage model);
        Task<Dictionary<ulong, AiUser>> GetUsage();
    }
}