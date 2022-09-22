using HuppyService.Core.Abstraction;
using HuppyService.Core.Entities;
using HuppyService.Core.Models;

namespace HuppyService.Core.Interfaces.IRepositories
{
    public interface ICommandLogRepository : IRepository<int, CommandLog>
    {

    }
}