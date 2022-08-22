using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IUserRepository : IRepository<ulong, User>
    {
        Task<Dictionary<ulong, string?>> GetUsersForCacheAsync();
    }
}