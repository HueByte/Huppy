using Discord.Interactions;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class ServerRepository : IServerRepository
    {
        private readonly HuppyDbContext _context;
        public ServerRepository(HuppyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Server>> GetAll()
        {
            return await _context.Servers.Include(e => e.Rooms)
                                         .ToListAsync();
        }

        public async Task AddOneAsync(Server server)
        {
            await _context.Servers.AddAsync(server);
            await _context.SaveChangesAsync();
        }

        public async Task AddOneAsync(ShardedInteractionContext DiscordContext)
        {
            if (await _context.Servers.AnyAsync(en => en.ID == DiscordContext.Guild.Id))
                throw new Exception($"Server with [{DiscordContext.Guild.Id}] ID already exists");

            Server server = new()
            {
                ID = DiscordContext.Guild.Id,
                GreetMessage = "Welcome {username}!",
                Rooms = new()
                {
                    OutputRoom = DiscordContext.Guild.DefaultChannel.Id,
                    NewsOutputRoom = 0,
                    GreetingRoom = 0
                },
                UseGreet = false,
                ServerName = DiscordContext.Guild.Name,
                RoleID = 0,
                AreNewsEnabled = false
            };

            await _context.Servers.AddAsync(server);
            await _context.SaveChangesAsync();
        }

        public async Task<Server> GetOrCreateAsync(ShardedInteractionContext DiscordContext)
        {
            var server = await _context.Servers.Include(e => e.Rooms)
                                               .FirstOrDefaultAsync(en => en.ID == DiscordContext.Guild.Id);

            if (server is not null)
            {
                if (server.Rooms is null)
                {
                    server.Rooms = new()
                    {
                        OutputRoom = DiscordContext.Guild.DefaultChannel.Id,
                        NewsOutputRoom = 0,
                        GreetingRoom = 0
                    };

                    _context.Servers.Update(server);
                    await _context.SaveChangesAsync();
                }

                return server;
            }

            server = new()
            {
                ID = DiscordContext.Guild.Id,
                GreetMessage = "Welcome {username}!",
                Rooms = new()
                {
                    OutputRoom = DiscordContext.Guild.DefaultChannel.Id,
                    NewsOutputRoom = 0,
                    GreetingRoom = 0
                },
                UseGreet = false,
                ServerName = DiscordContext.Guild.Name,
                RoleID = 0,
                AreNewsEnabled = false,
            };

            await _context.Servers.AddAsync(server);
            await _context.SaveChangesAsync();

            return server;
        }

        public async Task<Server?> GetOneAsync(ulong ID)
        {
            return await _context.Servers.Include(e => e.Rooms)
                                         .Where(en => en.ID == ID)
                                         .FirstOrDefaultAsync()!;
        }

        public async Task UpdateOne(Server server)
        {
            if (!await _context.Servers.AnyAsync(en => en.ID == server.ID))
                throw new Exception("Couldn't find this server");

            _context.Servers.Update(server);
            await _context.SaveChangesAsync();
        }
    }
}