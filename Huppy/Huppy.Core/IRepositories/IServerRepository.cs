using Discord.Interactions;
using Huppy.Core.Models;

namespace Huppy.Core.IRepositories
{
    public interface IServerRepository
    {
        Task AddOneAsync(Server server);
        Task AddOneAsync(ShardedInteractionContext DiscordContext);
        Task<List<Server>> GetAll();
        Task<Server> GetOneAsync(ulong ID);
        Task<Server> GetOrCreateAsync(ShardedInteractionContext DiscordContext);
        Task UpdateOne(Server server);
    }
}