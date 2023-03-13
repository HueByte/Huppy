using HuppyService.Core.Abstraction;
using HuppyService.Core.Models;

namespace HuppyService.Core.Interfaces.IRepositories
{
    public interface IServerRepository : IRepository<ulong, Server>
    {
    }
}