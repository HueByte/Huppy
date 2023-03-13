using HuppyService.Core.Abstraction;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HuppyService.Infrastructure.Repositories;

public class ServerRepository : BaseRepository<ulong, Server, HuppyDbContext>, IServerRepository
{
    public ServerRepository(HuppyDbContext context) : base(context) { }

    public override async Task<Server?> GetAsync(ulong id)
    {
        return await _context.Servers.Include(e => e.Rooms).FirstOrDefaultAsync(entry => entry.Id == id);
    }
}