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

    public async Task<Server> GetOrCreateAsync(ulong guildId, ulong defaultChannel, string guildName)
    {
        var server = await GetAsync(guildId);

        if (server is not null)
        {
            if (server.Rooms is null)
            {
                server.Rooms = new()
                {
                    OutputRoom = defaultChannel,
                    GreetingRoom = default
                };

                await base.UpdateAsync(server);
                await base.SaveChangesAsync();
            }

            return server;
        }

        server = new()
        {
            Id = guildId,
            GreetMessage = "Welcome {username}!",
            Rooms = new()
            {
                OutputRoom = defaultChannel,
                GreetingRoom = 0
            },
            ServerName = guildName,
            RoleID = 0,
            UseGreet = false,
        };

        await base.AddAsync(server);
        await base.SaveChangesAsync();

        return server;
    }
}