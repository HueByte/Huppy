using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IServerRepository
    {
        Task AddOneAsync(Server server);
        Task<Server> GetOneAsync(ulong ID);
        Task UpdateOne(Server server);
    }
}