using Huppy.Core.Models;
using Huppy.Kernel;

namespace Huppy.Core.Interfaces.IRepositories
{
    public interface IUserRepository : IRepository<ulong, User>
    {
        Task<Dictionary<ulong, string?>> GetUsersForCacheAsync();
    }
}