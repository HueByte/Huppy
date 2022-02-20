using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<Dictionary<ulong, string?>> GetUsersForCacheAsync();
    }
}