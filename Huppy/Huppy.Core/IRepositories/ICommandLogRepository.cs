using Huppy.Core.Entities;
using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface ICommandLogRepository
    {
        Task AddAsync(CommandLog commandLog);
        Task<Dictionary<ulong, AiUser>> GetAiUsage();
    }
}