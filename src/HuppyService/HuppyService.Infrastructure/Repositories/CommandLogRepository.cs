using HuppyService.Core.Abstraction;
using HuppyService.Core.Entities;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HuppyService.Infrastructure.Repositories
{
    public class CommandLogRepository : BaseRepository<int, CommandLog, HuppyDbContext>, ICommandLogRepository
    {
        public CommandLogRepository(HuppyDbContext context) : base(context) { }
    }
}