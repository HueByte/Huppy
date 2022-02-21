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

        public async Task AddOneAsync(Server server)
        {
            await _context.Servers.AddAsync(server);
            await _context.SaveChangesAsync();
        }

        public async Task AddOneAsync(ShardedInteractionContext DiscordContext)
        {
            var server = await _context.Servers.FirstOrDefaultAsync(en => en.ID == DiscordContext.Guild.Id);
            if (server is not null)
            {
                throw new Exception($"Server with [{DiscordContext.Guild.Id}] ID already exists");
            }

            server = new()
            {
                ID = DiscordContext.Guild.Id,
                GreetMessage = "",
                OutputRoom = DiscordContext.Guild.DefaultChannel.Id,
                UseGreet = false,
                ServerName = DiscordContext.Guild.Name,
                RoleID = 0
            };

            await _context.Servers.AddAsync(server);
            await _context.SaveChangesAsync();
        }

        public async Task<Server> GetOrCreateAsync(ShardedInteractionContext DiscordContext)
        {
            var server = await _context.Servers.FirstOrDefaultAsync(en => en.ID == DiscordContext.Guild.Id);
            if (server is not null)
            {
                return server;
            }

            server = new()
            {
                ID = DiscordContext.Guild.Id,
                GreetMessage = "",
                OutputRoom = DiscordContext.Guild.DefaultChannel.Id,
                UseGreet = false,
                ServerName = DiscordContext.Guild.Name,
                RoleID = 0
            };

            await _context.Servers.AddAsync(server);
            await _context.SaveChangesAsync();

            return server;
        }

        public async Task<Server?> GetOneAsync(ulong ID)
        {
            return await _context.Servers.Where(en => en.ID == ID).FirstOrDefaultAsync()!;
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