using Discord.Interactions;
using Huppy.Core.Interfaces;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.HuppyCacheStorage;
using Huppy.Kernel;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Middlewares
{
    public class DataSynchronizationMiddleware : IMiddleware
    {
        private readonly CacheStorageService _cacheService;
        private readonly IServerService _serverService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;
        public DataSynchronizationMiddleware(CacheStorageService cacheService, IUserRepository userRepository, ILogger<DataSynchronizationMiddleware> logger, IServerService serverService)
        {
            _cacheService = cacheService;
            _userRepository = userRepository;
            _logger = logger;
            _serverService = serverService;
        }

        public async Task BeforeAsync(ExtendedShardedInteractionContext context)
        {
            await SyncServerAsync(context);
            await SyncUserAsync(context);
        }

        public Task AfterAsync(ExtendedShardedInteractionContext context, ICommandInfo commandInfo, IResult result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// cache keeps all user IDs existing in database, if user is not present, he shall be added
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private async Task SyncUserAsync(ExtendedShardedInteractionContext ctx)
        {
            if (!_cacheService.CacheUsers.ContainsKey(ctx.User.Id))
            {
                _logger.LogInformation("Adding new user [{Username}] to database", ctx.User.Username);

                User user = new()
                {
                    Id = ctx.User.Id,
                    Username = ctx.User.Username,
                    JoinDate = DateTime.UtcNow,
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
                await _cacheService.AddCacheUser(ctx.User.Id, ctx.User.Username);
            }
        }


        /// <summary>
        /// fetch guilds from cache and check if server is already registered
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private async Task SyncServerAsync(ExtendedShardedInteractionContext ctx)
        {
            if (ctx.Guild is not null && !_cacheService.RegisteredGuildsIds.Contains(ctx.Guild.Id))
            {
                await _serverService.GetOrCreateAsync(ctx.Guild.Id, ctx.Guild.Name, ctx.Guild.DefaultChannel.Id);

                _cacheService.RegisteredGuildsIds.Add(ctx.Guild.Id);
            }
        }
    }
}