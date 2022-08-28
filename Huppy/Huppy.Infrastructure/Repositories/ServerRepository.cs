using Discord.Interactions;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class ServerRepository : BaseRepository<ulong, Server, HuppyDbContext>, IServerRepository
    {
        public ServerRepository(HuppyDbContext context) : base(context) { }

        public override async Task<Server?> GetAsync(ulong id)
        {
            return await _context.Servers.Include(e => e.Rooms).FirstOrDefaultAsync(entry => entry.Id == id);
        }

        public async Task<Server> GetOrCreateAsync(ShardedInteractionContext DiscordContext)
        {
            var server = await GetAsync(DiscordContext.Guild.Id);

            if (server is not null)
            {
                if (server.Rooms is null)
                {
                    server.Rooms = new()
                    {
                        OutputRoom = DiscordContext.Guild.DefaultChannel.Id,
                        GreetingRoom = default
                    };

                    await base.UpdateAsync(server);
                    await base.SaveChangesAsync();
                }

                return server;
            }

            server = new()
            {
                Id = DiscordContext.Guild.Id,
                GreetMessage = "Welcome {username}!",
                Rooms = new()
                {
                    OutputRoom = DiscordContext.Guild.DefaultChannel.Id,
                    GreetingRoom = 0
                },
                ServerName = DiscordContext.Guild.Name,
                RoleID = 0,
                UseGreet = false,
            };

            await base.AddAsync(server);
            await base.SaveChangesAsync();

            return server;
        }
    }
}