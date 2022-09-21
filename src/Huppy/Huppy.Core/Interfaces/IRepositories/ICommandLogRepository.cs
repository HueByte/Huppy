using Huppy.Core.Entities;
using Huppy.Core.Models;
using Huppy.Kernel;

namespace Huppy.Core.Interfaces.IRepositories
{
    [Obsolete]
    public interface ICommandLogRepository : IRepository<int, CommandLog>
    {
        Task<int> GetCount();
        Task<Dictionary<ulong, AiUser>> GetAiUsage();
    }
}