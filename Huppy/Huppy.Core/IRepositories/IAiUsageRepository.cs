using Huppy.Core.Entities;
using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IAiUsageRepository
    {
        Task AddAsync(AiUsage model);
        Task<List<AiUsage>> GetAll();
        Task<Dictionary<ulong, AiUser>> GetUsage();
        Task<List<ulong>> GetUserIDs();
        Task<Dictionary<ulong, string?>> GetUsersFromArray(List<ulong> users);
    }
}