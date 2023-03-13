using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Models;
using Huppy.Kernel.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<ulong, User, HuppyDbContext>, IUserRepository
    {
        public UserRepository(HuppyDbContext context) : base(context) { }

        public async Task<Dictionary<ulong, string?>> GetUsersForCacheAsync() =>
            await _context.Users?.ToDictionaryAsync(p => p.Id, p => p.Username)!;
    }
}