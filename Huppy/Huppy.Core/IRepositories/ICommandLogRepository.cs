using Huppy.Core.Entities;
using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface ICommandLogRepository : IRepository<int, CommandLog>
    {
        Task<int> GetCount();
        Task<Dictionary<ulong, AiUser>> GetAiUsage();
    }
}