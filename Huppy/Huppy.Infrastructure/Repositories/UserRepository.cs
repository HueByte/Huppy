using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HuppyDbContext _context;
        public UserRepository(HuppyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<ulong, string?>> GetUsersForCacheAsync() =>
            await _context.Users?.ToDictionaryAsync(p => p.Id, p => p.Username)!;
    }
}