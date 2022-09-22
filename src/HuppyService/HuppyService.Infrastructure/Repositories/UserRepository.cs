using HuppyService.Core.Abstraction;
using HuppyService.Core.Interfaces.IRepositories;
using HuppyService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HuppyService.Infrastructure.Repositories;

//public class UserRepository : BaseRepository<ulong, User, HuppyDbContext>, IUserRepository
//{
//    public UserRepository(HuppyDbContext context) : base(context) { }

//    public async Task<Dictionary<ulong, string?>> GetUsersForCacheAsync() =>
//        await _context.Users?.ToDictionaryAsync(p => p.Id, p => p.Username)!;
//}